using System.Data;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using RepairSystem.Model;
using RepairSystem.Service.DTOs;
using RepairSystem.Service.Helpers;

namespace RepairSystem.Service.Services;

/// <summary>Repair reception, assignment, work, and customer-visible progress.</summary>
public class RepairService
{
    private readonly AppDbContext _context;
    public RepairService(AppDbContext context) => _context = context;

    private sealed record Actor(long AccountId, string Role, long? EmployeeId);

    public async Task<RepairPageDto> GetAllAsync(long accountId, string? search = null, string? trangThai = null,
        long? kyThuatVienId = null, int page = 1, int pageSize = 20)
    {
        if (page < 1 || pageSize is < 1 or > 100 || page > int.MaxValue / pageSize) throw new ArgumentException("Trang phải hợp lệ; kích thước trang từ 1 đến 100.");
        if (search?.Length > 200) throw new ArgumentException("Từ khoá tìm kiếm tối đa 200 ký tự.");
        if (trangThai != null && !RepairWorkflow.States.Contains(trangThai)) throw new ArgumentException("Trạng thái phiếu không hợp lệ.");
        var actor = await GetActorAsync(accountId);
        var query = VisibleRepairs(actor);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var value = search.Trim();
            query = query.Where(p => p.MaPhieu.Contains(value) || p.ThietBi.TenThietBi.Contains(value)
                || p.ThietBi.KhachHang.HoTen.Contains(value));
        }
        if (trangThai != null) query = query.Where(p => p.TrangThai == trangThai);
        if (kyThuatVienId.HasValue) query = query.Where(p => p.KyThuatVienId == kyThuatVienId);
        var total = await query.CountAsync();
        var items = await query.OrderByDescending(p => p.NgayNhan).ThenByDescending(p => p.Id)
            .Skip((page - 1) * pageSize).Take(pageSize).Select(Summary).ToListAsync();
        return new RepairPageDto { Items = items, Total = total, Page = page, PageSize = pageSize };
    }

    public async Task<RepairDetailDto> GetByIdAsync(long accountId, long id)
    {
        var actor = await GetActorAsync(accountId);
        var summary = await VisibleRepairs(actor).Where(p => p.Id == id).Select(Summary).SingleOrDefaultAsync()
            ?? throw new KeyNotFoundException("Không tìm thấy phiếu sửa chữa.");
        var services = await _context.ChiTietDichVus.AsNoTracking().Where(s => s.PhieuId == id)
            .OrderBy(s => s.DichVuId).Select(s => new RepairServiceLineDto
            {
                DichVuId = s.DichVuId, TenDichVu = s.DichVu.TenDichVu, SoLuong = s.SoLuong,
                DonGiaThucTe = s.DonGiaThucTe, ThanhTien = s.SoLuong * s.DonGiaThucTe
            }).ToListAsync();
        // chi_tiet_xuat_kho refers to linh_kien through its composite stock key.
        var parts = await _context.ChiTietXuatKhos.AsNoTracking().Where(s => s.PhieuId == id)
            .OrderBy(s => s.Id).Select(s => new RepairPartLineDto
            {
                Id = s.Id, KhoId = s.KhoId, LinhKienId = s.LinhKienId,
                TenLinhKien = s.TonKho.LinhKien.TenLinhKien, SoLuong = s.SoLuongXuat,
                DonGia = s.DonGiaXuat, ThanhTien = s.SoLuongXuat * s.DonGiaXuat, BaoHanhDen = s.BaoHanhDen
            }).ToListAsync();
        var history = await _context.LichSuTrangThaiPhieus.AsNoTracking().Where(h => h.PhieuId == id)
            .OrderBy(h => h.ThoiGian).ThenBy(h => h.Id).Select(h => new RepairHistoryDto
            {
                Id = h.Id, TrangThaiCu = h.TrangThaiCu, TrangThaiMoi = h.TrangThaiMoi,
                NguoiThucHienId = h.NguoiThucHienId, ThoiGian = h.ThoiGian, GhiChu = h.GhiChu
            }).ToListAsync();
        var photos = await _context.AnhPhieuSuaChuas.AsNoTracking().Where(a => a.PhieuId == id)
            .OrderBy(a => a.Id).Select(a => new RepairPhotoDto
            { Id = a.Id, Loai = a.Loai, DuongDan = a.DuongDan, MoTa = a.MoTa, CreatedAt = a.CreatedAt }).ToListAsync();
        return new RepairDetailDto
        {
            Phieu = summary, DichVu = services, LinhKien = parts, LichSu = history, HinhAnh = photos,
            TienDichVu = services.Sum(s => s.ThanhTien), TienLinhKien = parts.Sum(s => s.ThanhTien)
        };
    }

    public async Task<RepairDetailDto> CreateAsync(long accountId, CreateRepairDto dto)
    {
        var actor = await GetActorAsync(accountId);
        RequireRole(actor, Roles.Admin, Roles.LeTan);
        if (actor.EmployeeId == null || !await _context.LeTans.AnyAsync(n => n.NhanVienId == actor.EmployeeId))
            throw new InvalidOperationException("Tiếp nhận phiếu cần tài khoản nhân viên lễ tân đang hoạt động, có hồ sơ lễ tân.");
        ValidateText(dto.TinhTrangLoi, 4000, "Tình trạng lỗi", required: true);
        ValidateText(dto.GhiChu, 4000, "Ghi chú");
        if (dto.ThietBiId < 1) throw new ArgumentException("Thiết bị không hợp lệ.");
        var now = DateTime.UtcNow;
        var appointment = NormalizeUtc(dto.NgayHenTra);
        if (appointment < now) throw new ArgumentException("Ngày hẹn trả không được trước ngày tiếp nhận.");
        await using var transaction = await BeginTransactionAsync();
        // Lock the device while checking for a current repair so simultaneous receptions cannot duplicate it.
        var device = _context.Database.IsSqlServer()
            ? await _context.ThietBis.FromSqlInterpolated($"SELECT * FROM dbo.thiet_bi WITH (UPDLOCK, ROWLOCK) WHERE id = {dto.ThietBiId}").SingleOrDefaultAsync()
            : await _context.ThietBis.SingleOrDefaultAsync(d => d.Id == dto.ThietBiId);
        if (device == null || device.DeletedAt != null || !await _context.KhachHangs.AnyAsync(k => k.Id == device.KhachHangId && k.DeletedAt == null))
            throw new KeyNotFoundException("Không tìm thấy thiết bị hoặc khách hàng đang hoạt động.");
        if (await _context.PhieuSuaChuas.AnyAsync(p => p.ThietBiId == device.Id && p.TrangThai != RepairWorkflow.Returned && p.TrangThai != RepairWorkflow.Cancelled))
            throw new InvalidOperationException("Thiết bị đang có phiếu chưa trả hoặc chưa huỷ.");
        var repair = new PhieuSuaChua
        {
            ThietBiId = device.Id, LeTanId = actor.EmployeeId.Value, NgayNhan = now,
            NgayHenTra = appointment, TinhTrangLoi = dto.TinhTrangLoi.Trim(), GhiChu = Clean(dto.GhiChu),
            TrangThai = RepairWorkflow.Received, CreatedAt = now, UpdatedAt = now
        };
        _context.PhieuSuaChuas.Add(repair);
        repair.LichSuTrangThaiPhieus.Add(new LichSuTrangThaiPhieu
        {
            TrangThaiMoi = RepairWorkflow.Received, NguoiThucHienId = actor.EmployeeId, ThoiGian = now,
            GhiChu = "Tiếp nhận thiết bị."
        });
        await _context.SaveChangesAsync();
        if (transaction != null) await transaction.CommitAsync();
        return await GetByIdAsync(accountId, repair.Id);
    }

    public async Task<RepairDetailDto> UpdateAsync(long accountId, long id, UpdateRepairDto dto)
    {
        ValidateText(dto.TinhTrangLoi, 4000, "Tình trạng lỗi", required: true);
        ValidateText(dto.GhiChu, 4000, "Ghi chú");
        return await MutateAsync(accountId, id, async (actor, repair) =>
        {
            RequireRole(actor, Roles.Admin, Roles.LeTan);
            await RequireMutableAsync(repair);
            var appointment = NormalizeUtc(dto.NgayHenTra);
            if (appointment < repair.NgayNhan) throw new ArgumentException("Ngày hẹn trả không được trước ngày tiếp nhận.");
            repair.NgayHenTra = appointment;
            repair.TinhTrangLoi = dto.TinhTrangLoi.Trim();
            repair.GhiChu = Clean(dto.GhiChu);
        });
    }

    public async Task<RepairDetailDto> AssignAsync(long accountId, long id, AssignRepairDto dto)
    {
        ValidateText(dto.GhiChu, 1000, "Ghi chú");
        return await MutateAsync(accountId, id, async (actor, repair) =>
        {
            RequireRole(actor, Roles.Admin, Roles.LeTan);
            await RequireMutableAsync(repair);
            var technician = await _context.KyThuatViens.AsNoTracking().AnyAsync(k => k.NhanVienId == dto.KyThuatVienId
                && k.NhanVien.DeletedAt == null && k.NhanVien.ChucVu == Roles.KyThuatVien
                && k.NhanVien.TaiKhoan.HoatDong && k.NhanVien.TaiKhoan.VaiTro == Roles.KyThuatVien);
            if (!technician) throw new ArgumentException("Kỹ thuật viên không tồn tại hoặc không hoạt động.");
            if (repair.KyThuatVienId != dto.KyThuatVienId)
            {
                repair.KyThuatVienId = dto.KyThuatVienId;
                AddHistory(repair, actor, repair.TrangThai, $"Phân công kỹ thuật viên #{dto.KyThuatVienId}. {Clean(dto.GhiChu)}".Trim());
            }
        });
    }

    public async Task<RepairDetailDto> ChangeStatusAsync(long accountId, long id, ChangeRepairStatusDto dto)
    {
        ValidateText(dto.KetQuaSuaChua, 4000, "Kết quả sửa chữa");
        ValidateText(dto.LyDoHuy, 4000, "Lý do huỷ");
        ValidateText(dto.GhiChu, 1000, "Ghi chú");
        return await MutateAsync(accountId, id, async (actor, repair) =>
        {
            if (dto.TrangThai is RepairWorkflow.Cancelled or RepairWorkflow.Returned)
                RequireRole(actor, Roles.Admin, Roles.LeTan);
            else
                RequireRole(actor, Roles.Admin, Roles.KyThuatVien);
            RequireAssignedTechnician(actor, repair);
            RepairWorkflow.ValidateTransition(repair.TrangThai, dto.TrangThai, repair.KyThuatVienId.HasValue,
                dto.KetQuaSuaChua ?? repair.KetQuaSuaChua, dto.LyDoHuy);
            if (dto.TrangThai == RepairWorkflow.Cancelled)
            {
                if (await _context.ChiTietXuatKhos.AnyAsync(c => c.PhieuId == repair.Id))
                    throw new InvalidOperationException("Phiếu đã xuất linh kiện; cần xử lý hoàn kho trước khi huỷ.");
                if (await _context.HoaDons.AnyAsync(h => h.PhieuId == repair.Id && h.TrangThai != "HUY"))
                    throw new InvalidOperationException("Không thể huỷ phiếu đang có hoá đơn hiệu lực.");
                repair.LyDoHuy = dto.LyDoHuy!.Trim();
            }
            if (dto.TrangThai == RepairWorkflow.Returned)
            {
                if (!await _context.HoaDons.AnyAsync(h => h.PhieuId == repair.Id && h.TrangThai == "DA_THANH_TOAN"))
                    throw new InvalidOperationException("Cần thanh toán hoá đơn trước khi trả thiết bị.");
                repair.NgayTra = DateTime.UtcNow;
            }
            if (dto.TrangThai == RepairWorkflow.Completed) repair.NgayHoanThanh = DateTime.UtcNow;
            if (dto.KetQuaSuaChua != null) repair.KetQuaSuaChua = Clean(dto.KetQuaSuaChua);
            var old = repair.TrangThai;
            repair.TrangThai = dto.TrangThai;
            AddHistory(repair, actor, old, Clean(dto.GhiChu));
        });
    }

    public async Task<RepairDetailDto> UpsertServiceAsync(long accountId, long id, long dichVuId, UpsertRepairServiceDto dto)
    {
        if (dto.SoLuong is < 1 or > 1000000 || dto.DonGiaThucTe is < 0 or > 999999999999.99m
            || (dto.DonGiaThucTe.HasValue && decimal.Round(dto.DonGiaThucTe.Value, 2) != dto.DonGiaThucTe.Value))
            throw new ArgumentException("Số lượng hoặc đơn giá dịch vụ không hợp lệ.");
        return await MutateAsync(accountId, id, async (actor, repair) =>
        {
            RequireRole(actor, Roles.Admin, Roles.LeTan, Roles.KyThuatVien);
            RequireAssignedTechnician(actor, repair);
            await RequireMutableAsync(repair);
            if (dto.DonGiaThucTe.HasValue && actor.Role != Roles.Admin)
                throw new UnauthorizedAccessException("Chỉ quản trị viên được điều chỉnh đơn giá dịch vụ.");
            var catalog = await _context.DichVus.AsNoTracking().SingleOrDefaultAsync(d => d.Id == dichVuId && d.DeletedAt == null)
                ?? throw new KeyNotFoundException("Không tìm thấy dịch vụ đang hoạt động.");
            var line = await _context.ChiTietDichVus.SingleOrDefaultAsync(c => c.PhieuId == id && c.DichVuId == dichVuId);
            var price = dto.DonGiaThucTe ?? line?.DonGiaThucTe ?? catalog.GiaNiemYet;
            if (price * dto.SoLuong > 999999999999.99m)
                throw new ArgumentException("Thành tiền dịch vụ vượt giới hạn lưu trữ.");
            if (line == null)
            {
                line = new ChiTietDichVu { PhieuId = id, DichVuId = dichVuId, CreatedAt = DateTime.UtcNow };
                _context.ChiTietDichVus.Add(line);
            }
            line.SoLuong = dto.SoLuong;
            line.DonGiaThucTe = price;
        });
    }

    public async Task<RepairDetailDto> RemoveServiceAsync(long accountId, long id, long dichVuId) =>
        await MutateAsync(accountId, id, async (actor, repair) =>
        {
            RequireRole(actor, Roles.Admin, Roles.LeTan, Roles.KyThuatVien);
            RequireAssignedTechnician(actor, repair);
            await RequireMutableAsync(repair);
            var line = await _context.ChiTietDichVus.SingleOrDefaultAsync(c => c.PhieuId == id && c.DichVuId == dichVuId)
                ?? throw new KeyNotFoundException("Không tìm thấy dịch vụ trên phiếu.");
            _context.ChiTietDichVus.Remove(line);
        });

    public async Task<RepairDetailDto> AddPhotoAsync(long accountId, long id, AddRepairPhotoDto dto)
    {
        if (dto.Loai is not ("LUC_TIEP_NHAN" or "SAU_SUA" or "KHAC")) throw new ArgumentException("Loại ảnh không hợp lệ.");
        if (dto.DuongDan?.Length > 500 || !Uri.TryCreate(dto.DuongDan, UriKind.Absolute, out var uri) || uri.Scheme != Uri.UriSchemeHttps || !string.IsNullOrEmpty(uri.UserInfo))
            throw new ArgumentException("Ảnh phải có đường dẫn HTTPS hợp lệ, tối đa 500 ký tự.");
        ValidateText(dto.MoTa, 255, "Mô tả ảnh");
        return await MutateAsync(accountId, id, (actor, repair) =>
        {
            RequireRole(actor, Roles.Admin, Roles.LeTan, Roles.KyThuatVien);
            RequireAssignedTechnician(actor, repair);
            if (repair.TrangThai is RepairWorkflow.Returned or RepairWorkflow.Cancelled)
                throw new InvalidOperationException("Không thể thay đổi ảnh trên phiếu đã kết thúc.");
            _context.AnhPhieuSuaChuas.Add(new AnhPhieuSuaChua
            { PhieuId = id, Loai = dto.Loai, DuongDan = uri.AbsoluteUri, MoTa = Clean(dto.MoTa), CreatedAt = DateTime.UtcNow });
            return Task.CompletedTask;
        });
    }

    public async Task<RepairDetailDto> RemovePhotoAsync(long accountId, long id, long photoId) =>
        await MutateAsync(accountId, id, async (actor, repair) =>
        {
            RequireRole(actor, Roles.Admin, Roles.LeTan, Roles.KyThuatVien);
            RequireAssignedTechnician(actor, repair);
            if (repair.TrangThai is RepairWorkflow.Returned or RepairWorkflow.Cancelled)
                throw new InvalidOperationException("Không thể thay đổi ảnh trên phiếu đã kết thúc.");
            var photo = await _context.AnhPhieuSuaChuas.SingleOrDefaultAsync(a => a.Id == photoId && a.PhieuId == id)
                ?? throw new KeyNotFoundException("Không tìm thấy ảnh trên phiếu.");
            _context.AnhPhieuSuaChuas.Remove(photo);
        });

    private async Task<RepairDetailDto> MutateAsync(long accountId, long id, Func<Actor, PhieuSuaChua, Task> action)
    {
        var actor = await GetActorAsync(accountId);
        if (actor.EmployeeId == null) throw new UnauthorizedAccessException("Chức năng chỉ dành cho nhân viên đang hoạt động.");
        await using var transaction = await BeginTransactionAsync();
        // The same parent-row lock is used by usp_lap_hoa_don and usp_xuat_kho.
        var repair = _context.Database.IsSqlServer()
            ? await _context.PhieuSuaChuas.FromSqlInterpolated($"SELECT * FROM dbo.phieu_sua_chua WITH (UPDLOCK, ROWLOCK) WHERE id = {id}").SingleOrDefaultAsync()
            : await _context.PhieuSuaChuas.SingleOrDefaultAsync(p => p.Id == id);
        if (repair == null) throw new KeyNotFoundException("Không tìm thấy phiếu sửa chữa.");
        await action(actor, repair);
        repair.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        if (transaction != null) await transaction.CommitAsync();
        return await GetByIdAsync(accountId, id);
    }

    private async Task<IDbContextTransaction?> BeginTransactionAsync() => _context.Database.IsRelational()
        ? await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable) : null;

    private async Task RequireMutableAsync(PhieuSuaChua repair)
    {
        if (!RepairWorkflow.IsMutable(repair.TrangThai)) throw new InvalidOperationException("Phiếu đã hoàn thành, đã trả hoặc đã huỷ; không thể sửa chi phí hoặc thông tin tiếp nhận.");
        if (await _context.HoaDons.AnyAsync(h => h.PhieuId == repair.Id && h.TrangThai != "HUY"))
            throw new InvalidOperationException("Phiếu đã có hoá đơn hiệu lực; không thể thay đổi chi phí.");
    }

    private async Task<Actor> GetActorAsync(long accountId)
    {
        var account = await _context.TaiKhoans.AsNoTracking().Where(a => a.Id == accountId && a.HoatDong)
            .Select(a => new { a.Id, a.VaiTro }).SingleOrDefaultAsync()
            ?? throw new UnauthorizedAccessException("Tài khoản không hoạt động.");
        if (account.VaiTro == Roles.KhachHang)
        {
            if (!await _context.KhachHangs.AnyAsync(k => k.TaiKhoanId == accountId && k.DeletedAt == null))
                throw new UnauthorizedAccessException("Hồ sơ khách hàng không hoạt động.");
            return new Actor(account.Id, account.VaiTro, null);
        }
        if (account.VaiTro is not (Roles.Admin or Roles.LeTan or Roles.ThuKho or Roles.KyThuatVien))
            throw new UnauthorizedAccessException("Vai trò không được phép truy cập phiếu sửa chữa.");
        var employeeId = await _context.NhanViens.AsNoTracking()
            .Where(n => n.TaiKhoanId == accountId && n.ChucVu == account.VaiTro && n.DeletedAt == null)
            .Select(n => (long?)n.Id).SingleOrDefaultAsync()
            ?? throw new UnauthorizedAccessException("Hồ sơ nhân viên không hoạt động.");
        return new Actor(account.Id, account.VaiTro, employeeId);
    }

    private IQueryable<PhieuSuaChua> VisibleRepairs(Actor actor)
    {
        var query = _context.PhieuSuaChuas.AsNoTracking();
        if (actor.Role == Roles.KhachHang)
            return query.Where(p => p.ThietBi.KhachHang.TaiKhoanId == actor.AccountId && p.ThietBi.KhachHang.DeletedAt == null);
        if (actor.Role == Roles.KyThuatVien) return query.Where(p => p.KyThuatVienId == actor.EmployeeId);
        return query;
    }

    private static void RequireRole(Actor actor, params string[] allowed)
    {
        if (!allowed.Contains(actor.Role)) throw new UnauthorizedAccessException("Bạn không có quyền thực hiện thao tác này.");
    }

    private static void RequireAssignedTechnician(Actor actor, PhieuSuaChua repair)
    {
        if (actor.Role == Roles.KyThuatVien && actor.EmployeeId != repair.KyThuatVienId)
            throw new UnauthorizedAccessException("Kỹ thuật viên chỉ được cập nhật phiếu được phân công.");
    }

    private void AddHistory(PhieuSuaChua repair, Actor actor, string oldStatus, string? note) =>
        _context.LichSuTrangThaiPhieus.Add(new LichSuTrangThaiPhieu
        { PhieuId = repair.Id, TrangThaiCu = oldStatus, TrangThaiMoi = repair.TrangThai, NguoiThucHienId = actor.EmployeeId, ThoiGian = DateTime.UtcNow, GhiChu = note });

    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    private static DateTime? NormalizeUtc(DateTime? value) => value?.Kind == DateTimeKind.Local ? value.Value.ToUniversalTime()
        : value.HasValue ? DateTime.SpecifyKind(value.Value, DateTimeKind.Utc) : null;

    private static void ValidateText(string? value, int maxLength, string label, bool required = false)
    {
        if (required && string.IsNullOrWhiteSpace(value)) throw new ArgumentException($"{label} là bắt buộc.");
        if (value?.Length > maxLength) throw new ArgumentException($"{label} tối đa {maxLength} ký tự.");
    }

    private static readonly Expression<Func<PhieuSuaChua, PhieuSuaChuaDto>> Summary = p => new PhieuSuaChuaDto
    {
        Id = p.Id, MaPhieu = p.MaPhieu, ThietBiId = p.ThietBiId, TenThietBi = p.ThietBi.TenThietBi,
        KhachHangId = p.ThietBi.KhachHangId, HoTenKhachHang = p.ThietBi.KhachHang.HoTen,
        LeTanId = p.LeTanId, KyThuatVienId = p.KyThuatVienId,
        TenKyThuatVien = p.KyThuatVien == null ? null : p.KyThuatVien.NhanVien.HoTen,
        NgayNhan = p.NgayNhan, NgayHenTra = p.NgayHenTra, NgayHoanThanh = p.NgayHoanThanh, NgayTra = p.NgayTra,
        TinhTrangLoi = p.TinhTrangLoi, KetQuaSuaChua = p.KetQuaSuaChua, TrangThai = p.TrangThai,
        LyDoHuy = p.LyDoHuy, GhiChu = p.GhiChu, CreatedAt = p.CreatedAt, UpdatedAt = p.UpdatedAt
    };
}
