using Microsoft.EntityFrameworkCore;
using RepairSystem.Model;
using RepairSystem.Service.DTOs;
using RepairSystem.Service.Helpers;
using RepairSystem.Service.IServices;

namespace RepairSystem.Service.Services;

public class NhanVienService : INhanVienService
{
    private readonly AppDbContext _context;

    public NhanVienService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<NhanVienDto>> GetAllAsync(string? chucVu = null, bool includeDeleted = false)
    {
        var query = _context.NhanViens
            .AsNoTracking()
            .Include(nv => nv.TaiKhoan)
            .Include(nv => nv.KyThuatViens)
            .AsQueryable();

        if (!includeDeleted)
            query = query.Where(nv => nv.DeletedAt == null);

        if (!string.IsNullOrWhiteSpace(chucVu))
            query = query.Where(nv => nv.ChucVu == chucVu);

        var list = await query.OrderBy(nv => nv.HoTen).ToListAsync();
        return list.Select(nv => ToDto(nv));
    }

    public async Task<NhanVienDto?> GetByIdAsync(long id)
    {
        var nv = await _context.NhanViens
            .AsNoTracking()
            .Include(n => n.TaiKhoan)
            .Include(n => n.KyThuatViens)
            .FirstOrDefaultAsync(n => n.Id == id && n.DeletedAt == null);

        return nv == null ? null : ToDto(nv);
    }

    public async Task<NhanVienDto> CreateAsync(CreateNhanVienDto dto)
    {
        ValidateChucVu(dto.ChucVu);

        var tenDangNhap = dto.TenDangNhap.Trim();
        if (await _context.TaiKhoans.AnyAsync(tk => tk.TenDangNhap == tenDangNhap))
            throw new InvalidOperationException($"Tên đăng nhập '{tenDangNhap}' đã tồn tại.");

        // Tất cả các bước nằm trong một transaction: lỗi ở bước nào cũng rollback, không để lại tài khoản mồ côi.
        await using var tx = await _context.Database.BeginTransactionAsync();
        try
        {
            // 1. Tài khoản
            var taiKhoan = new TaiKhoan
            {
                TenDangNhap = tenDangNhap,
                MatKhauHash = PasswordHelper.HashPassword(dto.MatKhau),
                VaiTro = dto.ChucVu,
                HoatDong = true
            };
            _context.TaiKhoans.Add(taiKhoan);
            await _context.SaveChangesAsync();

            // 2. Nhân viên
            var nhanVien = new NhanVien
            {
                HoTen = dto.HoTen.Trim(),
                SoDienThoai = dto.SoDienThoai.Trim(),
                Email = NullIfBlank(dto.Email),
                ChucVu = dto.ChucVu,
                TaiKhoanId = taiKhoan.Id
            };
            _context.NhanViens.Add(nhanVien);
            await _context.SaveChangesAsync();

            // 3. Dòng trong bảng con theo chức vụ (ADMIN không có bảng con)
            var chuyenMon = NullIfBlank(dto.ChuyenMon);
            switch (dto.ChucVu)
            {
                case Roles.KyThuatVien:
                    _context.KyThuatViens.Add(new KyThuatVien
                    {
                        NhanVienId = nhanVien.Id,
                        ChucVu = Roles.KyThuatVien,
                        ChuyenMon = chuyenMon
                    });
                    break;
                case Roles.LeTan:
                    _context.LeTans.Add(new LeTan { NhanVienId = nhanVien.Id, ChucVu = Roles.LeTan });
                    break;
                case Roles.ThuKho:
                    _context.ThuKhos.Add(new ThuKho { NhanVienId = nhanVien.Id, ChucVu = Roles.ThuKho });
                    break;
            }
            await _context.SaveChangesAsync();

            await tx.CommitAsync();

            nhanVien.TaiKhoan = taiKhoan;
            return ToDto(nhanVien, dto.ChucVu == Roles.KyThuatVien ? chuyenMon : null);
        }
        catch (DbUpdateException)
        {
            await tx.RollbackAsync();
            throw new InvalidOperationException(
                "Không thể tạo nhân viên: dữ liệu vi phạm ràng buộc (số điện thoại 8–15 ký tự, email hợp lệ và không trùng).");
        }
    }

    public async Task<NhanVienDto> UpdateAsync(long id, UpdateNhanVienDto dto)
    {
        var nv = await _context.NhanViens
            .Include(n => n.TaiKhoan)
            .Include(n => n.KyThuatViens)
            .FirstOrDefaultAsync(n => n.Id == id && n.DeletedAt == null)
            ?? throw new KeyNotFoundException($"Không tìm thấy nhân viên id={id}.");

        if (!string.IsNullOrWhiteSpace(dto.ChucVu) && dto.ChucVu != nv.ChucVu)
            throw new ArgumentException(
                "Hệ thống chưa hỗ trợ đổi chức vụ. Hãy vô hiệu hoá nhân viên cũ và tạo nhân viên mới với chức vụ mong muốn.");

        nv.HoTen = dto.HoTen.Trim();
        nv.SoDienThoai = dto.SoDienThoai.Trim();
        nv.Email = NullIfBlank(dto.Email);
        nv.UpdatedAt = DateTime.UtcNow;

        if (nv.ChucVu == Roles.KyThuatVien)
        {
            var ktv = nv.KyThuatViens.FirstOrDefault();
            if (ktv != null)
                ktv.ChuyenMon = NullIfBlank(dto.ChuyenMon);
        }

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            throw new ArgumentException("Dữ liệu vi phạm ràng buộc (số điện thoại 8–15 ký tự, email hợp lệ và không trùng).");
        }

        return ToDto(nv);
    }

    public async Task<bool> DeleteAsync(long id)
    {
        var nv = await _context.NhanViens
            .Include(n => n.TaiKhoan)
            .FirstOrDefaultAsync(n => n.Id == id && n.DeletedAt == null);

        if (nv == null) return false;

        var now = DateTime.UtcNow;
        nv.DeletedAt = now;
        nv.UpdatedAt = now;

        // Vô hiệu hoá tài khoản
        nv.TaiKhoan.HoatDong = false;
        nv.TaiKhoan.UpdatedAt = now;

        await _context.SaveChangesAsync();

        // Thu hồi phiên đăng nhập đang có
        await _context.RefreshTokens
            .Where(t => t.TaiKhoanId == nv.TaiKhoanId && t.ThuHoiLuc == null)
            .ExecuteUpdateAsync(s => s.SetProperty(t => t.ThuHoiLuc, (DateTime?)now));

        return true;
    }

    // ── helpers ────────────────────────────────────────────────────
    private static readonly string[] ValidChucVu =
        [Roles.Admin, Roles.LeTan, Roles.ThuKho, Roles.KyThuatVien];

    private static void ValidateChucVu(string chucVu)
    {
        if (!ValidChucVu.Contains(chucVu))
            throw new ArgumentException(
                $"Chức vụ '{chucVu}' không hợp lệ. Giá trị cho phép: {string.Join(", ", ValidChucVu)}");
    }

    private static string? NullIfBlank(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static NhanVienDto ToDto(NhanVien nv, string? chuyenMon = null) => new()
    {
        Id = nv.Id,
        MaNv = nv.MaNv,
        HoTen = nv.HoTen,
        SoDienThoai = nv.SoDienThoai,
        Email = nv.Email,
        ChucVu = nv.ChucVu,
        ChuyenMon = chuyenMon ?? nv.KyThuatViens.FirstOrDefault()?.ChuyenMon,
        TaiKhoanId = nv.TaiKhoanId,
        TenDangNhap = nv.TaiKhoan?.TenDangNhap,
        HoatDong = nv.TaiKhoan?.HoatDong ?? true,
        DangNhapCuoi = nv.TaiKhoan?.DangNhapCuoi,
        CreatedAt = nv.CreatedAt,
        UpdatedAt = nv.UpdatedAt
    };
}
