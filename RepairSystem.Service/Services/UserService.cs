using Microsoft.EntityFrameworkCore;
using RepairSystem.Model;
using RepairSystem.Service.DTOs;
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

    public async Task<UserDto?> GetUserByIdAsync(int userId)
    {
        var user = await _context.TaiKhoans.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
        return user == null ? null : MapToDto(user);
    }

    public async Task<UserDto?> GetUserByUsernameAsync(string username)
    {
        var name = username.Trim();
        var user = await _context.TaiKhoans.AsNoTracking().FirstOrDefaultAsync(u => u.TenDangNhap == name);
        return user == null ? null : MapToDto(user);
    }

    public Task<bool> DeleteUserAsync(int userId) => UpdateUserStatusAsync(userId, false);

    public async Task<bool> UpdateUserStatusAsync(int userId, bool isActive)
    {
        var user = await _context.TaiKhoans.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null) return false;

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
