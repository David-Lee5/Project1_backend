using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using RepairSystem.Model;
using RepairSystem.Service.DTOs;
using RepairSystem.Service.Helpers;

namespace RepairSystem.Service.Services;

/// <summary>Inventory mutations use the database's transaction-safe stock procedures.</summary>
public class InventoryService
{
    private readonly AppDbContext _context;
    private const decimal MaxMoney = 999999999999.99m;

    public InventoryService(AppDbContext context) => _context = context;

    public Task<List<WarehouseDto>> GetWarehousesAsync() => _context.Khos.AsNoTracking()
        .Where(x => x.DeletedAt == null).OrderBy(x => x.TenKho)
        .Select(x => new WarehouseDto { Id = x.Id, MaKho = x.MaKho, TenKho = x.TenKho, DiaDiem = x.DiaDiem, ThuKhoId = x.ThuKhoId }).ToListAsync();

    public async Task<WarehouseDto> GetWarehouseAsync(long id) => await _context.Khos.AsNoTracking()
        .Where(x => x.Id == id && x.DeletedAt == null)
        .Select(x => new WarehouseDto { Id = x.Id, MaKho = x.MaKho, TenKho = x.TenKho, DiaDiem = x.DiaDiem, ThuKhoId = x.ThuKhoId }).SingleOrDefaultAsync()
        ?? throw new KeyNotFoundException("Không tìm thấy kho.");

    public async Task<WarehouseDto> SaveWarehouseAsync(long? id, UpsertWarehouseDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.TenKho) || dto.TenKho.Trim().Length > 100 || dto.DiaDiem?.Length > 255)
            throw new ArgumentException("Thông tin kho không hợp lệ.");
        using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        if (dto.ThuKhoId.HasValue && !await ActiveKeepers().AnyAsync(x => x.NhanVienId == dto.ThuKhoId))
            throw new ArgumentException("Thủ kho phụ trách không hoạt động hoặc không đúng vai trò.");
        var warehouse = id.HasValue
            ? await _context.Khos.SingleOrDefaultAsync(x => x.Id == id && x.DeletedAt == null)
                ?? throw new KeyNotFoundException("Không tìm thấy kho.")
            : new Kho();
        warehouse.TenKho = dto.TenKho.Trim();
        warehouse.DiaDiem = NullIfBlank(dto.DiaDiem);
        warehouse.ThuKhoId = dto.ThuKhoId;
        warehouse.UpdatedAt = DateTime.UtcNow;
        if (!id.HasValue) _context.Khos.Add(warehouse);
        await _context.SaveChangesAsync();
        await transaction.CommitAsync();
        return await GetWarehouseAsync(warehouse.Id);
    }

    public async Task DeleteWarehouseAsync(long id)
    {
        using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var warehouse = await _context.Khos.FromSqlInterpolated($"SELECT * FROM dbo.kho WITH (UPDLOCK, HOLDLOCK) WHERE id = {id}")
            .SingleOrDefaultAsync() ?? throw new KeyNotFoundException("Không tìm thấy kho.");
        if (warehouse.DeletedAt != null) throw new KeyNotFoundException("Không tìm thấy kho.");
        if (await _context.TonKhos.AnyAsync(x => x.KhoId == id && x.SoLuongTon > 0) ||
            await _context.PhieuNhaps.AnyAsync(x => x.KhoId == id && x.TrangThai == "NHAP"))
            throw new InvalidOperationException("Không thể xóa kho còn tồn hàng hoặc phiếu nhập nháp.");
        warehouse.DeletedAt = warehouse.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        await transaction.CommitAsync();
    }

    public Task<List<StockDto>> GetStockAsync(long? khoId = null, bool lowOnly = false, int skip = 0, int take = 100)
    {
        ValidatePaging(skip, take);
        var query = _context.TonKhos.AsNoTracking().Where(x => x.Kho.DeletedAt == null && x.LinhKien.DeletedAt == null);
        if (khoId.HasValue) query = query.Where(x => x.KhoId == khoId);
        if (lowOnly) query = query.Where(x => x.SoLuongTon <= x.MucTonToiThieu);
        return query.OrderBy(x => x.KhoId).ThenBy(x => x.LinhKienId).Skip(skip).Take(take)
            .Select(x => new StockDto { KhoId = x.KhoId, TenKho = x.Kho.TenKho, LinhKienId = x.LinhKienId,
                MaLk = x.LinhKien.MaLk, TenLinhKien = x.LinhKien.TenLinhKien, SoLuongTon = x.SoLuongTon,
                MucTonToiThieu = x.MucTonToiThieu, NgayCapNhat = x.NgayCapNhat }).ToListAsync();
    }

    public async Task SetStockThresholdAsync(long khoId, long partId, int threshold, long accountId)
    {
        if (threshold < 0) throw new ArgumentException("Mức tồn tối thiểu không được âm.");
        await RequireKeeperAsync(accountId);
        var stock = await _context.TonKhos.SingleOrDefaultAsync(x => x.KhoId == khoId && x.LinhKienId == partId &&
            x.Kho.DeletedAt == null && x.LinhKien.DeletedAt == null) ?? throw new KeyNotFoundException("Không tìm thấy dòng tồn kho.");
        stock.MucTonToiThieu = threshold;
        stock.NgayCapNhat = DateTime.UtcNow;
        // This operation changes the warning threshold only; quantities are controlled by stock procedures.
        await _context.SaveChangesAsync();
    }

    public Task<List<StockMovementDto>> GetMovementsAsync(long? khoId = null, long? partId = null, int skip = 0, int take = 100)
    {
        ValidatePaging(skip, take);
        var query = _context.BienDongKhos.AsNoTracking().AsQueryable();
        if (khoId.HasValue) query = query.Where(x => x.KhoId == khoId);
        if (partId.HasValue) query = query.Where(x => x.LinhKienId == partId);
        return query.OrderByDescending(x => x.Id).Skip(skip).Take(take).Select(x => new StockMovementDto {
            Id = x.Id, KhoId = x.KhoId, LinhKienId = x.LinhKienId, Loai = x.Loai, SoLuongThayDoi = x.SoLuongThayDoi,
            SoLuongSau = x.SoLuongSau, PhieuNhapId = x.PhieuNhapId, ChiTietXuatKhoId = x.ChiTietXuatKhoId,
            NguoiThucHienId = x.NguoiThucHienId, ThoiGian = x.ThoiGian, LyDo = x.LyDo }).ToListAsync();
    }

    public Task<List<ReceiptDto>> GetReceiptsAsync(string? status = null, int skip = 0, int take = 100)
    {
        ValidatePaging(skip, take);
        if (status != null && status is not ("NHAP" or "DA_NHAP_KHO" or "HUY")) throw new ArgumentException("Trạng thái phiếu nhập không hợp lệ.");
        var query = _context.PhieuNhaps.AsNoTracking().AsQueryable();
        if (status != null) query = query.Where(x => x.TrangThai == status);
        return ProjectReceipts(query.OrderByDescending(x => x.Id).Skip(skip).Take(take)).ToListAsync();
    }

    public async Task<ReceiptDto> GetReceiptAsync(long id) => await ProjectReceipts(_context.PhieuNhaps.AsNoTracking().Where(x => x.Id == id))
        .SingleOrDefaultAsync() ?? throw new KeyNotFoundException("Không tìm thấy phiếu nhập.");

    public async Task<ReceiptDto> CreateReceiptAsync(UpsertReceiptDto dto, long accountId)
    {
        ValidateReceipt(dto);
        var keeper = await RequireKeeperAsync(accountId);
        using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        await ValidateReceiptReferencesAsync(dto);
        var receipt = new PhieuNhap { NhaCungCapId = dto.NhaCungCapId, KhoId = dto.KhoId, ThuKhoId = keeper,
            SoChungTuNcc = NullIfBlank(dto.SoChungTuNcc), GhiChu = NullIfBlank(dto.GhiChu), TrangThai = "NHAP",
            NgayNhap = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow,
            ChiTietNhapKhos = dto.ChiTiet.Select(x => new ChiTietNhapKho { LinhKienId = x.LinhKienId, SoLuongNhap = x.SoLuongNhap, DonGiaNhap = x.DonGiaNhap }).ToList() };
        _context.PhieuNhaps.Add(receipt);
        await _context.SaveChangesAsync();
        await transaction.CommitAsync();
        return await GetReceiptAsync(receipt.Id);
    }

    public async Task<ReceiptDto> UpdateReceiptAsync(long id, UpsertReceiptDto dto, long accountId)
    {
        ValidateReceipt(dto);
        await RequireKeeperAsync(accountId);
        using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var receipt = await LockReceiptAsync(id);
        if (receipt.TrangThai != "NHAP") throw new InvalidOperationException("Chỉ được sửa phiếu nhập nháp.");
        await ValidateReceiptReferencesAsync(dto);
        var existing = await _context.ChiTietNhapKhos.Where(x => x.PhieuNhapId == id).ToListAsync();
        foreach (var line in existing)
        {
            var input = dto.ChiTiet.SingleOrDefault(x => x.LinhKienId == line.LinhKienId);
            if (input == null) _context.ChiTietNhapKhos.Remove(line);
            else { line.SoLuongNhap = input.SoLuongNhap; line.DonGiaNhap = input.DonGiaNhap; }
        }
        foreach (var line in dto.ChiTiet.Where(x => !existing.Any(y => y.LinhKienId == x.LinhKienId)))
            _context.ChiTietNhapKhos.Add(new ChiTietNhapKho { PhieuNhapId = id, LinhKienId = line.LinhKienId, SoLuongNhap = line.SoLuongNhap, DonGiaNhap = line.DonGiaNhap });
        receipt.NhaCungCapId = dto.NhaCungCapId;
        receipt.KhoId = dto.KhoId;
        receipt.SoChungTuNcc = NullIfBlank(dto.SoChungTuNcc);
        receipt.GhiChu = NullIfBlank(dto.GhiChu);
        receipt.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        await transaction.CommitAsync();
        return await GetReceiptAsync(id);
    }

    public async Task<ReceiptDto> ConfirmReceiptAsync(long id, long accountId)
    {
        var keeper = await RequireKeeperAsync(accountId);
        using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var receipt = await LockReceiptAsync(id);
        if (receipt.TrangThai != "NHAP") throw new InvalidOperationException("Chỉ được xác nhận phiếu nhập nháp.");
        await RequireActiveWarehouseAsync(receipt.KhoId);
        if (!await _context.NhaCungCaps.AnyAsync(x => x.Id == receipt.NhaCungCapId && x.DeletedAt == null))
            throw new InvalidOperationException("Nhà cung cấp đã ngừng hoạt động.");
        if (await _context.ChiTietNhapKhos.AnyAsync(x => x.PhieuNhapId == id && x.LinhKien.DeletedAt != null))
            throw new InvalidOperationException("Phiếu nhập có linh kiện đã ngừng hoạt động.");
        try
        {
            await _context.Database.ExecuteSqlRawAsync("EXEC dbo.usp_xac_nhan_nhap_kho @phieu_nhap_id, @nhan_vien_id",
                new SqlParameter("@phieu_nhap_id", SqlDbType.BigInt) { Value = id }, new SqlParameter("@nhan_vien_id", SqlDbType.BigInt) { Value = keeper });
        }
        catch (SqlException ex) when (ex.Number == 8115)
        {
            // Overflow is detected inside the same transaction as the stock update; no partial receipt can persist.
            throw new InvalidOperationException("Số lượng tồn kho vượt giới hạn.", ex);
        }
        // The stored procedure mutates state directly; do not re-save its stale tracked state.
        _context.Entry(receipt).State = EntityState.Detached;
        await _context.PhieuNhaps.Where(x => x.Id == id).ExecuteUpdateAsync(x => x.SetProperty(y => y.UpdatedAt, DateTime.UtcNow));
        await transaction.CommitAsync();
        return await GetReceiptAsync(id);
    }

    public async Task<ReceiptDto> CancelReceiptAsync(long id, long accountId)
    {
        await RequireKeeperAsync(accountId);
        using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var receipt = await LockReceiptAsync(id);
        if (receipt.TrangThai != "NHAP") throw new InvalidOperationException("Chỉ được hủy phiếu nhập nháp; phiếu đã nhập kho không thể hủy.");
        receipt.TrangThai = "HUY";
        receipt.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        await transaction.CommitAsync();
        return await GetReceiptAsync(id);
    }

    public async Task<DispatchDto> DispatchAsync(DispatchRequestDto dto, long accountId)
    {
        if (dto.PhieuId <= 0 || dto.KhoId <= 0 || dto.LinhKienId <= 0 || dto.SoLuong <= 0)
            throw new ArgumentException("Thông tin xuất kho không hợp lệ.");
        var keeper = await RequireKeeperAsync(accountId);
        using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        await RequireActiveWarehouseAsync(dto.KhoId);
        var part = await _context.LinhKiens.AsNoTracking().SingleOrDefaultAsync(x => x.Id == dto.LinhKienId && x.DeletedAt == null)
            ?? throw new KeyNotFoundException("Không tìm thấy linh kiện đang hoạt động.");
        if (part.DonGiaBan * dto.SoLuong > MaxMoney) throw new ArgumentException("Thành tiền xuất kho vượt giới hạn.");
        var output = new SqlParameter("@chi_tiet_id", SqlDbType.BigInt) { Direction = ParameterDirection.Output };
        await _context.Database.ExecuteSqlRawAsync("EXEC dbo.usp_xuat_kho @phieu_id, @kho_id, @linh_kien_id, @so_luong, @thu_kho_id, @chi_tiet_id OUTPUT",
            new SqlParameter("@phieu_id", SqlDbType.BigInt) { Value = dto.PhieuId }, new SqlParameter("@kho_id", SqlDbType.BigInt) { Value = dto.KhoId },
            new SqlParameter("@linh_kien_id", SqlDbType.BigInt) { Value = dto.LinhKienId }, new SqlParameter("@so_luong", SqlDbType.Int) { Value = dto.SoLuong },
            new SqlParameter("@thu_kho_id", SqlDbType.BigInt) { Value = keeper }, output);
        var id = (long)output.Value;
        await transaction.CommitAsync();
        return await GetDispatchAsync(id);
    }

    public Task<List<DispatchDto>> GetDispatchesAsync(long? phieuId = null, int skip = 0, int take = 100)
    {
        ValidatePaging(skip, take);
        var query = _context.ChiTietXuatKhos.AsNoTracking().AsQueryable();
        if (phieuId.HasValue) query = query.Where(x => x.PhieuId == phieuId);
        return ProjectDispatches(query.OrderByDescending(x => x.Id).Skip(skip).Take(take)).ToListAsync();
    }

    public async Task<DispatchDto> GetDispatchAsync(long id) => await ProjectDispatches(_context.ChiTietXuatKhos.AsNoTracking().Where(x => x.Id == id))
        .SingleOrDefaultAsync() ?? throw new KeyNotFoundException("Không tìm thấy chi tiết xuất kho.");

    private IQueryable<ThuKho> ActiveKeepers() => _context.ThuKhos.Where(x => x.NhanVien.DeletedAt == null &&
        x.NhanVien.TaiKhoan.HoatDong && x.NhanVien.ChucVu == Roles.ThuKho && x.NhanVien.TaiKhoan.VaiTro == Roles.ThuKho);

    private async Task<long> RequireKeeperAsync(long accountId) => await ActiveKeepers().Where(x => x.NhanVien.TaiKhoanId == accountId)
        .Select(x => (long?)x.NhanVienId).SingleOrDefaultAsync() ?? throw new UnauthorizedAccessException("Thao tác yêu cầu tài khoản thủ kho đang hoạt động.");

    private async Task RequireActiveWarehouseAsync(long id)
    {
        // Shared lock on the warehouse lasts through this transaction and prevents soft-delete races.
        var warehouse = await _context.Khos.FromSqlInterpolated($"SELECT * FROM dbo.kho WITH (HOLDLOCK) WHERE id = {id}").AsNoTracking().SingleOrDefaultAsync();
        if (warehouse == null || warehouse.DeletedAt != null) throw new KeyNotFoundException("Không tìm thấy kho đang hoạt động.");
    }

    private async Task ValidateReceiptReferencesAsync(UpsertReceiptDto dto)
    {
        await RequireActiveWarehouseAsync(dto.KhoId);
        if (!await _context.NhaCungCaps.AnyAsync(x => x.Id == dto.NhaCungCapId && x.DeletedAt == null))
            throw new ArgumentException("Nhà cung cấp không tồn tại hoặc đã ngừng hoạt động.");
        var ids = dto.ChiTiet.Select(x => x.LinhKienId).ToArray();
        if (await _context.LinhKiens.CountAsync(x => ids.Contains(x.Id) && x.DeletedAt == null) != ids.Length)
            throw new ArgumentException("Phiếu nhập có linh kiện không tồn tại hoặc đã ngừng hoạt động.");
    }

    private async Task<PhieuNhap> LockReceiptAsync(long id) => await _context.PhieuNhaps
        .FromSqlInterpolated($"SELECT * FROM dbo.phieu_nhap WITH (UPDLOCK, ROWLOCK) WHERE id = {id}").SingleOrDefaultAsync()
        ?? throw new KeyNotFoundException("Không tìm thấy phiếu nhập.");

    private static void ValidatePaging(int skip, int take)
    {
        if (skip < 0 || take is < 1 or > 200) throw new ArgumentException("Phân trang yêu cầu skip >= 0 và take từ 1 đến 200.");
    }

    private static void ValidateReceipt(UpsertReceiptDto dto)
    {
        if (dto.NhaCungCapId <= 0 || dto.KhoId <= 0 || dto.ChiTiet == null || dto.ChiTiet.Count is < 1 or > 200 ||
            dto.SoChungTuNcc?.Length > 50 || dto.GhiChu?.Length > 4000)
            throw new ArgumentException("Thông tin phiếu nhập không hợp lệ.");
        if (dto.ChiTiet.Any(x => x == null || x.LinhKienId <= 0 || x.SoLuongNhap <= 0 || x.DonGiaNhap < 0 || x.DonGiaNhap > MaxMoney || decimal.Round(x.DonGiaNhap, 2) != x.DonGiaNhap || x.DonGiaNhap * x.SoLuongNhap > MaxMoney))
            throw new ArgumentException("Chi tiết nhập yêu cầu số lượng dương, đơn giá không âm và tối đa hai chữ số thập phân; thành tiền trong giới hạn decimal(14,2).");
        if (dto.ChiTiet.Select(x => x.LinhKienId).Distinct().Count() != dto.ChiTiet.Count)
            throw new ArgumentException("Mỗi linh kiện chỉ được xuất hiện một lần trong phiếu nhập.");
        if (dto.ChiTiet.Sum(x => x.DonGiaNhap * x.SoLuongNhap) > MaxMoney)
            throw new ArgumentException("Tổng tiền phiếu nhập vượt giới hạn.");
    }

    private static string? NullIfBlank(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static IQueryable<ReceiptDto> ProjectReceipts(IQueryable<PhieuNhap> query) => query.Select(x => new ReceiptDto {
        Id = x.Id, MaPn = x.MaPn, NhaCungCapId = x.NhaCungCapId, KhoId = x.KhoId, ThuKhoId = x.ThuKhoId,
        SoChungTuNcc = x.SoChungTuNcc, NgayNhap = x.NgayNhap, TrangThai = x.TrangThai, GhiChu = x.GhiChu,
        TongTien = x.ChiTietNhapKhos.Sum(y => (decimal?)y.SoLuongNhap * y.DonGiaNhap) ?? 0,
        ChiTiet = x.ChiTietNhapKhos.OrderBy(y => y.LinhKienId).Select(y => new ReceiptLineDto { LinhKienId = y.LinhKienId,
            TenLinhKien = y.LinhKien.TenLinhKien, SoLuongNhap = y.SoLuongNhap, DonGiaNhap = y.DonGiaNhap, ThanhTien = y.SoLuongNhap * y.DonGiaNhap }).ToList() });

    private static IQueryable<DispatchDto> ProjectDispatches(IQueryable<ChiTietXuatKho> query) => query.Select(x => new DispatchDto {
        Id = x.Id, PhieuId = x.PhieuId, KhoId = x.KhoId, LinhKienId = x.LinhKienId, ThuKhoId = x.ThuKhoId,
        SoLuongXuat = x.SoLuongXuat, DonGiaXuat = x.DonGiaXuat, ThanhTien = x.SoLuongXuat * x.DonGiaXuat, NgayXuat = x.NgayXuat, BaoHanhDen = x.BaoHanhDen });
}
