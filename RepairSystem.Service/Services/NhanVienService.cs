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
            .Include(nv => nv.TaiKhoan)
            .AsQueryable();

        if (!includeDeleted)
            query = query.Where(nv => nv.DeletedAt == null);

        if (!string.IsNullOrWhiteSpace(chucVu))
            query = query.Where(nv => nv.ChucVu == chucVu);

        var list = await query.OrderBy(nv => nv.HoTen).ToListAsync();
        return list.Select(ToDto);
    }

    public async Task<NhanVienDto?> GetByIdAsync(long id)
    {
        var nv = await _context.NhanViens
            .Include(n => n.TaiKhoan)
            .FirstOrDefaultAsync(n => n.Id == id && n.DeletedAt == null);

        return nv == null ? null : ToDto(nv);
    }

    public async Task<NhanVienDto> CreateAsync(CreateNhanVienDto dto)
    {
        // Kiểm tra tên đăng nhập
        if (await _context.TaiKhoans.AnyAsync(tk => tk.TenDangNhap == dto.TenDangNhap))
            throw new InvalidOperationException($"Tên đăng nhập '{dto.TenDangNhap}' đã tồn tại.");

        ValidateChucVu(dto.ChucVu);

        // Tạo tài khoản trước
        var taiKhoan = new TaiKhoan
        {
            TenDangNhap = dto.TenDangNhap,
            MatKhauHash = PasswordHelper.HashPassword(dto.MatKhau),
            VaiTro = dto.ChucVu,
            HoatDong = true
        };
        _context.TaiKhoans.Add(taiKhoan);
        await _context.SaveChangesAsync();

        // Tạo nhân viên
        var nhanVien = new NhanVien
        {
            HoTen = dto.HoTen,
            SoDienThoai = dto.SoDienThoai,
            Email = dto.Email,
            ChucVu = dto.ChucVu,
            TaiKhoanId = taiKhoan.Id
        };
        _context.NhanViens.Add(nhanVien);
        await _context.SaveChangesAsync();

        nhanVien.TaiKhoan = taiKhoan;
        return ToDto(nhanVien);
    }

    public async Task<NhanVienDto> UpdateAsync(long id, UpdateNhanVienDto dto)
    {
        var nv = await _context.NhanViens
            .Include(n => n.TaiKhoan)
            .FirstOrDefaultAsync(n => n.Id == id && n.DeletedAt == null)
            ?? throw new KeyNotFoundException($"Không tìm thấy nhân viên id={id}.");

        ValidateChucVu(dto.ChucVu);

        nv.HoTen = dto.HoTen;
        nv.SoDienThoai = dto.SoDienThoai;
        nv.Email = dto.Email;
        nv.ChucVu = dto.ChucVu;
        nv.UpdatedAt = DateTime.UtcNow;

        // Đồng bộ vai trò tài khoản
        nv.TaiKhoan.VaiTro = dto.ChucVu;

        await _context.SaveChangesAsync();
        return ToDto(nv);
    }

    public async Task<bool> DeleteAsync(long id)
    {
        var nv = await _context.NhanViens
            .Include(n => n.TaiKhoan)
            .FirstOrDefaultAsync(n => n.Id == id && n.DeletedAt == null);

        if (nv == null) return false;

        nv.DeletedAt = DateTime.UtcNow;
        nv.UpdatedAt = DateTime.UtcNow;

        // Vô hiệu hoá tài khoản
        nv.TaiKhoan.HoatDong = false;

        await _context.SaveChangesAsync();
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

    private static NhanVienDto ToDto(NhanVien nv) => new()
    {
        Id = nv.Id,
        MaNv = nv.MaNv,
        HoTen = nv.HoTen,
        SoDienThoai = nv.SoDienThoai,
        Email = nv.Email,
        ChucVu = nv.ChucVu,
        TaiKhoanId = nv.TaiKhoanId,
        TenDangNhap = nv.TaiKhoan?.TenDangNhap,
        CreatedAt = nv.CreatedAt,
        UpdatedAt = nv.UpdatedAt
    };
}
