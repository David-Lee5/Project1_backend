using RepairSystem.Data;
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
        var users = await _context.TaiKhoans
            .Include(u => u.Role)
            .ThenInclude(r => r!.Permissions)
            .ToListAsync();

        return users.Select(MapToDto).ToList();
    }

    public async Task<UserDto?> GetUserByIdAsync(int userId)
    {
        var user = await _context.TaiKhoans
            .Include(u => u.Role)
            .ThenInclude(r => r!.Permissions)
            .FirstOrDefaultAsync(u => u.TaiKhoanId == userId);

        return user == null ? null : MapToDto(user);
    }

    public async Task<UserDto?> GetUserByUsernameAsync(string username)
    {
        var user = await _context.TaiKhoans
            .Include(u => u.Role)
            .ThenInclude(r => r!.Permissions)
            .FirstOrDefaultAsync(u => u.Username == username);

        return user == null ? null : MapToDto(user);
    }

    public async Task<UserDto> CreateUserAsync(UserDto userDto, string password)
    {
        if (await _context.TaiKhoans.AnyAsync(u => u.Username == userDto.Username))
            throw new InvalidOperationException("Username already exists");

        var user = new TaiKhoan
        {
            Username = userDto.Username,
            Email = userDto.Email,
            PasswordHash = PasswordHelper.HashPassword(password),
            RoleId = userDto.RoleId,
            TrangThaiHoatDong = true
        };

        _context.TaiKhoans.Add(user);
        await _context.SaveChangesAsync();

        return MapToDto(user);
    }

    public async Task<UserDto> UpdateUserAsync(int userId, UserDto userDto)
    {
        var user = await _context.TaiKhoans.FindAsync(userId);
        if (user == null)
            throw new KeyNotFoundException($"User with id {userId} not found");

        user.Username = userDto.Username;
        user.Email = userDto.Email;
        user.RoleId = userDto.RoleId;
        user.UpdatedDate = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return MapToDto(user);
    }

    public async Task<bool> DeleteUserAsync(int userId)
    {
        var user = await _context.TaiKhoans.FindAsync(userId);
        if (user == null)
            return false;

        _context.TaiKhoans.Remove(user);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> AssignRoleToUserAsync(int userId, int roleId)
    {
        var user = await _context.TaiKhoans.FindAsync(userId);
        if (user == null)
            return false;

        var role = await _context.Set<Role>().FindAsync(roleId);
        if (role == null)
            return false;

        user.RoleId = roleId;
        user.UpdatedDate = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> UpdateUserStatusAsync(int userId, bool isActive)
    {
        var user = await _context.TaiKhoans.FindAsync(userId);
        if (user == null)
            return false;

        user.TrangThaiHoatDong = isActive;
        user.UpdatedDate = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return true;
    }

    private UserDto MapToDto(TaiKhoan user)
    {
        var permissions = user.Role?.Permissions?.Select(p => p.Name).ToList() ?? new List<string>();

        return new UserDto
        {
            TaiKhoanId = user.TaiKhoanId,
            Username = user.Username,
            Email = user.Email,
            RoleId = user.RoleId,
            RoleName = user.Role?.Name,
            Permissions = permissions,
            IsActive = user.TrangThaiHoatDong ?? false
        };
    }
}
