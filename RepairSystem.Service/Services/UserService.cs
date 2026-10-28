using RepairSystem.Model;
using RepairSystem.Service.DTOs;
using RepairSystem.Service.Helpers;
using RepairSystem.Service.IServices;
using Microsoft.EntityFrameworkCore;

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
        var users = await _context.TaiKhoans.ToListAsync();
        return users.Select(MapToDto);
    }

    public async Task<UserDto?> GetUserByIdAsync(int userId)
    {
        var user = await _context.TaiKhoans.FindAsync((long)userId);
        return user == null ? null : MapToDto(user);
    }

    public async Task<UserDto?> GetUserByUsernameAsync(string username)
    {
        var user = await _context.TaiKhoans
            .FirstOrDefaultAsync(u => u.TenDangNhap == username);
        return user == null ? null : MapToDto(user);
    }

    public async Task<UserDto> CreateUserAsync(UserDto userDto, string password)
    {
        if (await _context.TaiKhoans.AnyAsync(u => u.TenDangNhap == userDto.TenDangNhap))
            throw new InvalidOperationException("Tên đăng nhập đã tồn tại.");

        var user = new TaiKhoan
        {
            TenDangNhap = userDto.TenDangNhap,
            MatKhauHash = PasswordHelper.HashPassword(password),
            VaiTro = userDto.VaiTro,
            HoatDong = true
        };

        _context.TaiKhoans.Add(user);
        await _context.SaveChangesAsync();

        return MapToDto(user);
    }

    public async Task<UserDto> UpdateUserAsync(int userId, UserDto userDto)
    {
        var user = await _context.TaiKhoans.FindAsync((long)userId)
            ?? throw new KeyNotFoundException($"Không tìm thấy tài khoản id={userId}.");

        user.VaiTro = userDto.VaiTro;
        user.HoatDong = userDto.HoatDong;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return MapToDto(user);
    }

    public async Task<bool> DeleteUserAsync(int userId)
    {
        var user = await _context.TaiKhoans.FindAsync((long)userId);
        if (user == null) return false;

        // Soft-disable (không xoá hard vì có FK nhân viên)
        user.HoatDong = false;
        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> AssignRoleToUserAsync(int userId, int roleId)
    {
        // TaiKhoan không có RoleId trong DB thực — vai trò lưu thẳng vào cột VaiTro (string)
        // Phương thức này giữ lại để tương thích interface, không thực hiện gì
        return await Task.FromResult(false);
    }

    public async Task<bool> UpdateUserStatusAsync(int userId, bool isActive)
    {
        var user = await _context.TaiKhoans.FindAsync((long)userId);
        if (user == null) return false;

        user.HoatDong = isActive;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
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
