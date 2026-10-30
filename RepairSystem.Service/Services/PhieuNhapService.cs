using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using RepairSystem.Model;
using RepairSystem.Service.DTOs;
using RepairSystem.Service.Helpers;
using RepairSystem.Service.IServices;

namespace RepairSystem.Service.Services;

public class PhieuNhapService : IPhieuNhapService
{
    // Giá trị cột phieu_nhap.trang_thai (có CHECK trong DB)
    private const string Nhap = "NHAP";
    private const string DaNhapKho = "DA_NHAP_KHO";
    private const string Huy = "HUY";

    private readonly AppDbContext _context;

    public PhieuNhapService(AppDbContext context)
    {
        _context = context;
    }

    // ── Tạo phiếu nhập ─────────────────────────────────────────────
    public async Task<PhieuNhapChiTietDto> TaoMoiAsync(TaoPhieuNhapDto dto, long taiKhoanId, string vaiTro)
    {
        if (dto.ChiTiet == null || dto.ChiTiet.Count == 0)
            throw new ArgumentException("Phiếu nhập phải có ít nhất một dòng linh kiện.");

        var trung = dto.ChiTiet.GroupBy(c => c.LinhKienId).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
        if (trung.Count > 0)
            throw new ArgumentException(
                $"Linh kiện bị lặp trong phiếu: {string.Join(", ", trung)}. Hãy gộp thành một dòng.");

        if (!await _context.NhaCungCaps.AnyAsync(n => n.Id == dto.NhaCungCapId && n.DeletedAt == null))
            throw new ArgumentException($"Nhà cung cấp id={dto.NhaCungCapId} không tồn tại.");

        var kho = await _context.Khos.AsNoTracking()
            .FirstOrDefaultAsync(k => k.Id == dto.KhoId && k.DeletedAt == null)
            ?? throw new ArgumentException($"Kho id={dto.KhoId} không tồn tại.");

        var lkIds = dto.ChiTiet.Select(c => c.LinhKienId).ToList();
        var lkHopLe = await _context.LinhKiens
            .Where(l => lkIds.Contains(l.Id) && l.DeletedAt == null)
            .Select(l => l.Id)
            .ToListAsync();
        var lkThieu = lkIds.Except(lkHopLe).ToList();
        if (lkThieu.Count > 0)
            throw new ArgumentException($"Linh kiện không tồn tại hoặc đã bị xoá: {string.Join(", ", lkThieu)}.");

        // Người thực hiện (cần cho thủ kho và cho bước xác nhận)
        long? nhanVienId = null;
        if (vaiTro == Roles.ThuKho || dto.XacNhanNgay)
            nhanVienId = await GetNhanVienIdAsync(taiKhoanId);

        // Thủ kho chịu trách nhiệm phiếu
        long thuKhoId;
        if (vaiTro == Roles.ThuKho)
        {
            thuKhoId = nhanVienId!.Value;
        }
        else
        {
            thuKhoId = dto.ThuKhoId ?? kho.ThuKhoId
                ?? throw new ArgumentException("Kho chưa có thủ kho phụ trách. Vui lòng chỉ định thuKhoId.");
        }

        if (!await _context.ThuKhos.AnyAsync(t => t.NhanVienId == thuKhoId))
            throw new ArgumentException($"thuKhoId={thuKhoId} không phải là thủ kho.");

        var phieu = new PhieuNhap
        {
            NhaCungCapId = dto.NhaCungCapId,
            KhoId = dto.KhoId,
            ThuKhoId = thuKhoId,
            SoChungTuNcc = NullIfBlank(dto.SoChungTuNcc),
            NgayNhap = dto.NgayNhap ?? DateTime.UtcNow,
            TrangThai = Nhap,
            GhiChu = NullIfBlank(dto.GhiChu)
        };
        foreach (var c in dto.ChiTiet)
        {
            phieu.ChiTietNhapKhos.Add(new ChiTietNhapKho
            {
                LinhKienId = c.LinhKienId,
                SoLuongNhap = c.SoLuongNhap,
                DonGiaNhap = c.DonGiaNhap
            });
        }

        // Lưu phiếu và (nếu cần) nhập kho trong một transaction: lỗi ở bước nào cũng rollback.
        await using var tx = await _context.Database.BeginTransactionAsync();
        try
        {
            _context.PhieuNhaps.Add(phieu);
            await _context.SaveChangesAsync();

            if (dto.XacNhanNgay)
                await ChayXacNhanAsync(phieu.Id, nhanVienId!.Value);

            await tx.CommitAsync();
        }
        catch (SqlException ex)
        {
            await tx.RollbackAsync();
            throw MapSqlException(ex);
        }
        catch (DbUpdateException)
        {
            await tx.RollbackAsync();
            throw new InvalidOperationException(
                "Không thể lưu phiếu nhập do vi phạm ràng buộc dữ liệu (số lượng phải > 0, đơn giá >= 0).");
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }

        return (await GetByIdAsync(phieu.Id))!;
    }

    // ── Xác nhận nhập kho phiếu nháp ───────────────────────────────
    public async Task<PhieuNhapChiTietDto> XacNhanAsync(long id, long taiKhoanId)
    {
        var nhanVienId = await GetNhanVienIdAsync(taiKhoanId);

        try
        {
            await ChayXacNhanAsync(id, nhanVienId);
        }
        catch (SqlException ex)
        {
            throw MapSqlException(ex);
        }

        return (await GetByIdAsync(id))!;
    }

    // ── Huỷ phiếu nháp ─────────────────────────────────────────────
    public async Task<PhieuNhapChiTietDto> HuyAsync(long id)
    {
        // Cập nhật có điều kiện trong một câu lệnh để tránh chạy đua với thao tác xác nhận.
        var now = DateTime.UtcNow;
        var soDong = await _context.PhieuNhaps
            .Where(p => p.Id == id && p.TrangThai == Nhap)
            .ExecuteUpdateAsync(s => s
                .SetProperty(p => p.TrangThai, Huy)
                .SetProperty(p => p.UpdatedAt, now));

        if (soDong == 0)
        {
            var trangThai = await _context.PhieuNhaps
                .Where(p => p.Id == id)
                .Select(p => p.TrangThai)
                .FirstOrDefaultAsync();

            if (trangThai == null)
                throw new KeyNotFoundException($"Không tìm thấy phiếu nhập id={id}.");

            throw new InvalidOperationException(
                $"Chỉ huỷ được phiếu ở trạng thái {Nhap} (hiện: {trangThai}). " +
                "Phiếu đã nhập kho không thể huỷ vì tồn kho đã được cộng.");
        }

        return (await GetByIdAsync(id))!;
    }

    // ── Truy vấn ───────────────────────────────────────────────────
    public async Task<IEnumerable<PhieuNhapDto>> GetAllAsync(
        string? trangThai = null, long? nhaCungCapId = null, long? khoId = null,
        DateTime? tuNgay = null, DateTime? denNgay = null)
    {
        var query = _context.PhieuNhaps.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(trangThai))
            query = query.Where(p => p.TrangThai == trangThai);
        if (nhaCungCapId.HasValue)
            query = query.Where(p => p.NhaCungCapId == nhaCungCapId.Value);
        if (khoId.HasValue)
            query = query.Where(p => p.KhoId == khoId.Value);
        if (tuNgay.HasValue)
            query = query.Where(p => p.NgayNhap >= tuNgay.Value.Date);
        if (denNgay.HasValue)
        {
            var den = denNgay.Value.Date.AddDays(1);   // gồm cả ngày cuối
            query = query.Where(p => p.NgayNhap < den);
        }

        return await query
            .OrderByDescending(p => p.NgayNhap)
            .ThenByDescending(p => p.Id)
            .Select(p => new PhieuNhapDto
            {
                Id = p.Id,
                MaPn = p.MaPn,
                NhaCungCapId = p.NhaCungCapId,
                TenNcc = p.NhaCungCap.TenNcc,
                KhoId = p.KhoId,
                TenKho = p.Kho.TenKho,
                ThuKhoId = p.ThuKhoId,
                TenThuKho = p.ThuKho.NhanVien.HoTen,
                SoChungTuNcc = p.SoChungTuNcc,
                NgayNhap = p.NgayNhap,
                TrangThai = p.TrangThai,
                GhiChu = p.GhiChu,
                TongTienNhap = p.ChiTietNhapKhos.Sum(c => c.SoLuongNhap * c.DonGiaNhap),
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt
            })
            .ToListAsync();
    }

    public async Task<PhieuNhapChiTietDto?> GetByIdAsync(long id)
    {
        var phieu = await _context.PhieuNhaps
            .AsNoTracking()
            .Where(p => p.Id == id)
            .Select(p => new PhieuNhapDto
            {
                Id = p.Id,
                MaPn = p.MaPn,
                NhaCungCapId = p.NhaCungCapId,
                TenNcc = p.NhaCungCap.TenNcc,
                KhoId = p.KhoId,
                TenKho = p.Kho.TenKho,
                ThuKhoId = p.ThuKhoId,
                TenThuKho = p.ThuKho.NhanVien.HoTen,
                SoChungTuNcc = p.SoChungTuNcc,
                NgayNhap = p.NgayNhap,
                TrangThai = p.TrangThai,
                GhiChu = p.GhiChu,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt
            })
            .FirstOrDefaultAsync();

        if (phieu == null) return null;

        var chiTiet = await _context.ChiTietNhapKhos
            .AsNoTracking()
            .Where(c => c.PhieuNhapId == id)
            .OrderBy(c => c.LinhKienId)
            .Select(c => new DongNhapKhoDto
            {
                LinhKienId = c.LinhKienId,
                MaLk = c.LinhKien.MaLk,
                TenLinhKien = c.LinhKien.TenLinhKien,
                DonViTinh = c.LinhKien.DonViTinh,
                SoLuongNhap = c.SoLuongNhap,
                DonGiaNhap = c.DonGiaNhap,
                ThanhTien = c.SoLuongNhap * c.DonGiaNhap
            })
            .ToListAsync();

        phieu.TongTienNhap = chiTiet.Sum(c => c.ThanhTien);

        return new PhieuNhapChiTietDto { Phieu = phieu, ChiTiet = chiTiet };
    }

    // ── helpers ────────────────────────────────────────────────────
    /// <summary>Gọi stored procedure đã có sẵn trong DB: khoá phiếu, cộng tồn kho, ghi bien_dong_kho, đổi trạng thái.</summary>
    private async Task ChayXacNhanAsync(long phieuNhapId, long nhanVienId)
    {
        await _context.Database.ExecuteSqlInterpolatedAsync(
            $"EXEC dbo.usp_xac_nhan_nhap_kho @phieu_nhap_id = {phieuNhapId}, @nhan_vien_id = {nhanVienId}");
    }

    private async Task<long> GetNhanVienIdAsync(long taiKhoanId)
    {
        var id = await _context.NhanViens
            .Where(n => n.TaiKhoanId == taiKhoanId && n.DeletedAt == null)
            .Select(n => (long?)n.Id)
            .FirstOrDefaultAsync();

        return id ?? throw new InvalidOperationException("Tài khoản hiện tại chưa gắn với hồ sơ nhân viên.");
    }

    /// <summary>Đổi lỗi nghiệp vụ do THROW 5000x trong procedure thành exception có ý nghĩa.</summary>
    private static Exception MapSqlException(SqlException ex) => ex.Number switch
    {
        50001 => new KeyNotFoundException(ex.Message),
        50002 or 50003 => new InvalidOperationException(ex.Message),
        _ => ex
    };

    private static string? NullIfBlank(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
