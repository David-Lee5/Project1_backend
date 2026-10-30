using Microsoft.EntityFrameworkCore;
using RepairSystem.Model;
using RepairSystem.Service.DTOs;
using RepairSystem.Service.Helpers;
using RepairSystem.Service.IServices;

namespace RepairSystem.Service.Services;

public class UserService : IUserService
{
    private readonly AppDbContext _context;

    public UserService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
    {
        var users = await _context.TaiKhoans
            .AsNoTracking()
            .OrderBy(u => u.Id)
            .ToListAsync();

        return users.Select(MapToDto).ToList();
    }

    public async Task<UserDto?> GetUserByIdAsync(long userId)
    {
        var user = await _context.TaiKhoans.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
        return user == null ? null : MapToDto(user);
    }

    public async Task<UserDto?> GetUserByUsernameAsync(string username)
    {
        if (string.IsNullOrWhiteSpace(username)) return null;
        var name = username.Trim();
        var user = await _context.TaiKhoans.AsNoTracking().FirstOrDefaultAsync(u => u.TenDangNhap == name);
        return user == null ? null : MapToDto(user);
    }

    public Task<bool> DeleteUserAsync(long userId) => UpdateUserStatusAsync(userId, false);

    public async Task<bool> UpdateUserStatusAsync(long userId, bool isActive)
    {
        await using var tx = await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
        var user = await _context.TaiKhoans.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null) return false;

        if (!isActive && user.HoatDong && user.VaiTro == Roles.Admin &&
            !await _context.TaiKhoans.AnyAsync(u => u.Id != userId && u.VaiTro == Roles.Admin && u.HoatDong))
            throw new InvalidOperationException("Không thể vô hiệu hoá quản trị viên đang hoạt động cuối cùng.");

        if (isActive && user.VaiTro != Roles.KhachHang &&
            !await _context.NhanViens.AnyAsync(n => n.TaiKhoanId == userId && n.DeletedAt == null))
            throw new InvalidOperationException("Không thể kích hoạt tài khoản nhân viên đã bị xoá.");
        if (isActive && user.VaiTro == Roles.KhachHang &&
            !await _context.KhachHangs.AnyAsync(k => k.TaiKhoanId == userId && k.DeletedAt == null))
            throw new InvalidOperationException("Không thể kích hoạt tài khoản khách hàng không còn hồ sơ hoạt động.");

        var now = DateTime.UtcNow;
        user.HoatDong = isActive;
        if (isActive)
        {
            user.KhoaDen = null;
            user.SoLanDangNhapSai = 0;
        }
        user.UpdatedAt = now;
        await _context.SaveChangesAsync();

        if (!isActive)
        {
            await _context.RefreshTokens
                .Where(t => t.TaiKhoanId == user.Id && t.ThuHoiLuc == null)
                .ExecuteUpdateAsync(s => s.SetProperty(t => t.ThuHoiLuc, (DateTime?)now));
        }

        await tx.CommitAsync();
        return true;
    }

    private static UserDto MapToDto(TaiKhoan user) => new()
    {
        TaiKhoanId = user.Id,
        TenDangNhap = user.TenDangNhap,
        VaiTro = user.VaiTro,
        HoatDong = user.HoatDong
    };
}
