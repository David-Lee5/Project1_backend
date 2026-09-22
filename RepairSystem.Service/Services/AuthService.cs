using RepairSystem.Data;
using RepairSystem.Data.Repository.IRepository;
using RepairSystem.Model;
using RepairSystem.Service.DTOs;
using RepairSystem.Service.Helpers;
using RepairSystem.Service.IServices;
using Microsoft.EntityFrameworkCore;

namespace RepairSystem.Service.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly ITokenService _tokenService;
    private readonly IUserService _userService;

    public AuthService(AppDbContext context, ITokenService tokenService, IUserService userService)
    {
        _context = context;
        _tokenService = tokenService;
        _userService = userService;
    }

    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request)
    {
        var user = await _context.TaiKhoans
            .Include(u => u.Role)
            .ThenInclude(r => r!.Permissions)
            .FirstOrDefaultAsync(u => u.Username == request.Username);

        if (user == null || !PasswordHelper.VerifyPassword(request.Password, user.PasswordHash))
        {
            return new LoginResponseDto
            {
                Success = false,
                Message = "Invalid username or password"
            };
        }

        if (!user.TrangThaiHoatDong.HasValue || !user.TrangThaiHoatDong.Value)
        {
            return new LoginResponseDto
            {
                Success = false,
                Message = "User account is inactive"
            };
        }

        var permissions = user.Role?.Permissions?.Select(p => p.Name).ToList() ?? new List<string>();

        var accessToken = _tokenService.GenerateAccessToken(
            user.TaiKhoanId,
            user.Username,
            user.Role?.Name,
            permissions
        );

        var refreshToken = _tokenService.GenerateRefreshToken();
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

        await _context.SaveChangesAsync();

        var userDto = new UserDto
        {
            TaiKhoanId = user.TaiKhoanId,
            Username = user.Username,
            Email = user.Email,
            RoleId = user.RoleId,
            RoleName = user.Role?.Name,
            Permissions = permissions,
            IsActive = user.TrangThaiHoatDong ?? false
        };

        return new LoginResponseDto
        {
            Success = true,
            Message = "Login successful",
            Token = new TokenDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresIn = DateTime.UtcNow.AddMinutes(15)
            },
            User = userDto
        };
    }

    public async Task<bool> RegisterAsync(string username, string email, string password, int? roleId = null)
    {
        if (await _context.TaiKhoans.AnyAsync(u => u.Username == username))
        {
            return false;
        }

        var user = new TaiKhoan
        {
            Username = username,
            Email = email,
            PasswordHash = PasswordHelper.HashPassword(password),
            RoleId = roleId,
            TrangThaiHoatDong = true
        };

        _context.TaiKhoans.Add(user);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<TokenDto> RefreshTokenAsync(string refreshToken)
    {
        var user = await _context.TaiKhoans
            .Include(u => u.Role)
            .ThenInclude(r => r!.Permissions)
            .FirstOrDefaultAsync(u => u.RefreshToken == refreshToken);

        if (user == null || user.RefreshTokenExpiryTime < DateTime.UtcNow)
        {
            throw new UnauthorizedAccessException("Invalid or expired refresh token");
        }

        var permissions = user.Role?.Permissions?.Select(p => p.Name).ToList() ?? new List<string>();

        var accessToken = _tokenService.GenerateAccessToken(
            user.TaiKhoanId,
            user.Username,
            user.Role?.Name,
            permissions
        );

        var newRefreshToken = _tokenService.GenerateRefreshToken();
        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

        await _context.SaveChangesAsync();

        return new TokenDto
        {
            AccessToken = accessToken,
            RefreshToken = newRefreshToken,
            ExpiresIn = DateTime.UtcNow.AddMinutes(15)
        };
    }

    public async Task<bool> LogoutAsync(int userId)
    {
        var user = await _context.TaiKhoans.FindAsync(userId);
        if (user != null)
        {
            user.RefreshToken = null;
            user.RefreshTokenExpiryTime = null;
            await _context.SaveChangesAsync();
            return true;
        }

        return false;
    }

    public async Task<UserDto?> GetUserByIdAsync(int userId)
    {
        var user = await _context.TaiKhoans
            .Include(u => u.Role)
            .ThenInclude(r => r!.Permissions)
            .FirstOrDefaultAsync(u => u.TaiKhoanId == userId);

        if (user == null)
            return null;

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

    public async Task<bool> ChangePasswordAsync(int userId, string oldPassword, string newPassword)
    {
        var user = await _context.TaiKhoans.FindAsync(userId);
        if (user == null || !PasswordHelper.VerifyPassword(oldPassword, user.PasswordHash))
        {
            return false;
        }

        user.PasswordHash = PasswordHelper.HashPassword(newPassword);
        user.UpdatedDate = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return true;
    }
}
