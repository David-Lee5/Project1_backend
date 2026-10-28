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

    public AuthService(AppDbContext context, ITokenService tokenService)
    {
        _context = context;
        _tokenService = tokenService;
    }

    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request)
    {
        var user = await _context.TaiKhoans
            .FirstOrDefaultAsync(u => u.TenDangNhap == request.Username);

        if (user == null || !PasswordHelper.VerifyPassword(request.Password, user.MatKhauHash))
        {
            return new LoginResponseDto
            {
                Success = false,
                Message = "Tên đăng nhập hoặc mật khẩu không đúng."
            };
        }

        if (!user.HoatDong)
        {
            return new LoginResponseDto
            {
                Success = false,
                Message = "Tài khoản đã bị vô hiệu hoá."
            };
        }

        if (user.KhoaDen.HasValue && user.KhoaDen > DateTime.UtcNow)
        {
            return new LoginResponseDto
            {
                Success = false,
                Message = $"Tài khoản bị khoá đến {user.KhoaDen:dd/MM/yyyy HH:mm}."
            };
        }

        // Cập nhật thời gian đăng nhập cuối & reset số lần sai
        user.DangNhapCuoi = DateTime.UtcNow;
        user.SoLanDangNhapSai = 0;

        var accessToken = _tokenService.GenerateAccessToken(
            (int)user.Id, user.TenDangNhap, user.VaiTro, permissions: null);

        // Lưu refresh token mới vào bảng refresh_token
        var rawRefreshToken = _tokenService.GenerateRefreshToken();
        var refreshTokenHash = PasswordHelper.HashPassword(rawRefreshToken); // hash để lưu an toàn

        _context.RefreshTokens.Add(new RefreshToken
        {
            TaiKhoanId = user.Id,
            TokenHash = refreshTokenHash,
            HetHan = DateTime.UtcNow.AddDays(7)
        });

        await _context.SaveChangesAsync();

        return new LoginResponseDto
        {
            Success = true,
            Message = "Đăng nhập thành công.",
            Token = new TokenDto
            {
                AccessToken = accessToken,
                RefreshToken = rawRefreshToken,
                ExpiresIn = DateTime.UtcNow.AddMinutes(15)
            },
            User = new UserDto
            {
                TaiKhoanId = user.Id,
                TenDangNhap = user.TenDangNhap,
                VaiTro = user.VaiTro,
                HoatDong = user.HoatDong
            }
        };
    }

    public async Task<bool> RegisterAsync(string username, string email, string password, int? roleId = null)
    {
        if (await _context.TaiKhoans.AnyAsync(u => u.TenDangNhap == username))
            return false;

        var user = new TaiKhoan
        {
            TenDangNhap = username,
            MatKhauHash = PasswordHelper.HashPassword(password),
            VaiTro = Roles.KhachHang,
            HoatDong = true
        };

        _context.TaiKhoans.Add(user);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<TokenDto> RefreshTokenAsync(string rawRefreshToken)
    {
        var tokenHash = PasswordHelper.HashPassword(rawRefreshToken);

        var storedToken = await _context.RefreshTokens
            .Include(rt => rt.TaiKhoan)
            .FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash && rt.ThuHoiLuc == null);

        if (storedToken == null || storedToken.HetHan < DateTime.UtcNow)
            throw new UnauthorizedAccessException("Refresh token không hợp lệ hoặc đã hết hạn.");

        var user = storedToken.TaiKhoan;

        // Thu hồi token cũ
        storedToken.ThuHoiLuc = DateTime.UtcNow;

        // Tạo token mới
        var newAccessToken = _tokenService.GenerateAccessToken(
            (int)user.Id, user.TenDangNhap, user.VaiTro, permissions: null);
        var newRawRefresh = _tokenService.GenerateRefreshToken();

        _context.RefreshTokens.Add(new RefreshToken
        {
            TaiKhoanId = user.Id,
            TokenHash = PasswordHelper.HashPassword(newRawRefresh),
            HetHan = DateTime.UtcNow.AddDays(7)
        });

        await _context.SaveChangesAsync();

        return new TokenDto
        {
            AccessToken = newAccessToken,
            RefreshToken = newRawRefresh,
            ExpiresIn = DateTime.UtcNow.AddMinutes(15)
        };
    }

    public async Task<bool> LogoutAsync(int userId)
    {
        // Thu hồi tất cả refresh token còn hiệu lực của user
        var tokens = await _context.RefreshTokens
            .Where(rt => rt.TaiKhoanId == userId && rt.ThuHoiLuc == null)
            .ToListAsync();

        foreach (var t in tokens)
            t.ThuHoiLuc = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<UserDto?> GetUserByIdAsync(int userId)
    {
        var user = await _context.TaiKhoans.FindAsync((long)userId);
        if (user == null) return null;

        return new UserDto
        {
            TaiKhoanId = user.Id,
            TenDangNhap = user.TenDangNhap,
            VaiTro = user.VaiTro,
            HoatDong = user.HoatDong
        };
    }

    public async Task<bool> ChangePasswordAsync(int userId, string oldPassword, string newPassword)
    {
        var user = await _context.TaiKhoans.FindAsync((long)userId);
        if (user == null || !PasswordHelper.VerifyPassword(oldPassword, user.MatKhauHash))
            return false;

        user.MatKhauHash = PasswordHelper.HashPassword(newPassword);
        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }
}
