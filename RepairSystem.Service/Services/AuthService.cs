using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using RepairSystem.Model;
using RepairSystem.Service.DTOs;
using RepairSystem.Service.Helpers;
using RepairSystem.Service.IServices;

namespace RepairSystem.Service.Services;

public class AuthService : IAuthService
{
    private const int MaxFailedAttempts = 5;
    private static readonly TimeSpan LockDuration = TimeSpan.FromMinutes(15);
    private static readonly TimeSpan RefreshTokenLifetime = TimeSpan.FromDays(7);
    private const string InvalidCredentials = "Tên đăng nhập hoặc mật khẩu không đúng.";

    private readonly AppDbContext _context;
    private readonly ITokenService _tokenService;

    public AuthService(AppDbContext context, ITokenService tokenService)
    {
        _context = context;
        _tokenService = tokenService;
    }

    // ── Đăng nhập ───────────────────────────────────────────────────
    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request)
    {
        var username = request.Username.Trim();
        var user = await _context.TaiKhoans.FirstOrDefaultAsync(u => u.TenDangNhap == username);

        if (user == null)
            return Fail(InvalidCredentials);

        var now = DateTime.UtcNow;

        if (user.KhoaDen.HasValue && user.KhoaDen.Value > now)
        {
            var phut = Math.Ceiling((user.KhoaDen.Value - now).TotalMinutes);
            return Fail($"Tài khoản tạm thời bị khoá do nhập sai nhiều lần. Vui lòng thử lại sau {phut} phút.");
        }

        if (!PasswordHelper.VerifyPassword(request.Password, user.MatKhauHash))
        {
            user.SoLanDangNhapSai++;
            if (user.SoLanDangNhapSai >= MaxFailedAttempts)
            {
                user.KhoaDen = now.Add(LockDuration);
                user.SoLanDangNhapSai = 0;
            }
            user.UpdatedAt = now;
            await _context.SaveChangesAsync();

            return Fail(InvalidCredentials);
        }

        if (!user.HoatDong)
            return Fail("Tài khoản đã bị vô hiệu hoá.");

        user.SoLanDangNhapSai = 0;
        user.KhoaDen = null;
        user.DangNhapCuoi = now;
        user.UpdatedAt = now;

        var token = await IssueTokensAsync(user, now);

        return new LoginResponseDto
        {
            Success = true,
            Message = "Đăng nhập thành công.",
            Token = token,
            User = new UserDto
            {
                TaiKhoanId = user.Id,
                TenDangNhap = user.TenDangNhap,
                VaiTro = user.VaiTro,
                HoatDong = user.HoatDong
            }
        };
    }

    // ── Đăng ký khách hàng ──────────────────────────────────────────
    public async Task<(bool Success, string Message)> RegisterAsync(RegisterRequestDto request)
    {
        var username = request.Username.Trim();
        var email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim();

        if (await _context.TaiKhoans.AnyAsync(u => u.TenDangNhap == username))
            return (false, "Tên đăng nhập đã tồn tại.");

        if (email != null && await _context.KhachHangs.AnyAsync(k => k.Email == email && k.DeletedAt == null))
            return (false, "Email đã được sử dụng.");

        await using var tx = await _context.Database.BeginTransactionAsync();
        try
        {
            var taiKhoan = new TaiKhoan
            {
                TenDangNhap = username,
                MatKhauHash = PasswordHelper.HashPassword(request.Password),
                VaiTro = Roles.KhachHang,
                HoatDong = true
            };
            _context.TaiKhoans.Add(taiKhoan);
            await _context.SaveChangesAsync();

            _context.KhachHangs.Add(new KhachHang
            {
                HoTen = request.HoTen.Trim(),
                SoDienThoai = request.SoDienThoai.Trim(),
                Email = email,
                TaiKhoanId = taiKhoan.Id,
                VaiTroTk = Roles.KhachHang
            });
            await _context.SaveChangesAsync();

            await tx.CommitAsync();
            return (true, "Đăng ký thành công.");
        }
        catch (DbUpdateException)
        {
            await tx.RollbackAsync();
            return (false, "Không thể tạo tài khoản. Vui lòng kiểm tra lại số điện thoại và email.");
        }
    }

    // ── Refresh token ───────────────────────────────────────────────
    public async Task<TokenDto> RefreshTokenAsync(string rawRefreshToken)
    {
        var tokenHash = HashToken(rawRefreshToken);
        var now = DateTime.UtcNow;

        var stored = await _context.RefreshTokens
            .Include(rt => rt.TaiKhoan)
            .FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash);

        if (stored == null)
            throw new UnauthorizedAccessException("Refresh token không hợp lệ.");

        // Token đã bị thu hồi mà vẫn được dùng lại -> nghi bị lộ, thu hồi toàn bộ phiên của tài khoản.
        if (stored.ThuHoiLuc != null)
        {
            await RevokeAllAsync(stored.TaiKhoanId, now);
            throw new UnauthorizedAccessException("Refresh token đã bị thu hồi. Vui lòng đăng nhập lại.");
        }

        if (stored.HetHan < now)
            throw new UnauthorizedAccessException("Refresh token đã hết hạn.");

        var user = stored.TaiKhoan;
        if (!user.HoatDong || (user.KhoaDen.HasValue && user.KhoaDen.Value > now))
            throw new UnauthorizedAccessException("Tài khoản hiện không khả dụng.");

        // Xoay vòng: thu hồi token cũ, cấp token mới
        stored.ThuHoiLuc = now;
        return await IssueTokensAsync(user, now);
    }

    // ── Đăng xuất ───────────────────────────────────────────────────
    public async Task<bool> LogoutAsync(int userId)
    {
        await RevokeAllAsync(userId, DateTime.UtcNow);
        return true;
    }

    public async Task<UserDto?> GetUserByIdAsync(int userId)
    {
        var user = await _context.TaiKhoans.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
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
        var user = await _context.TaiKhoans.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null || !PasswordHelper.VerifyPassword(oldPassword, user.MatKhauHash))
            return false;

        var now = DateTime.UtcNow;
        user.MatKhauHash = PasswordHelper.HashPassword(newPassword);
        user.UpdatedAt = now;
        await _context.SaveChangesAsync();

        // Buộc các thiết bị khác đăng nhập lại
        await RevokeAllAsync(userId, now);
        return true;
    }

    // ── helpers ─────────────────────────────────────────────────────
    private async Task<TokenDto> IssueTokensAsync(TaiKhoan user, DateTime now)
    {
        var rawRefreshToken = _tokenService.GenerateRefreshToken();

        _context.RefreshTokens.Add(new RefreshToken
        {
            TaiKhoanId = user.Id,
            TokenHash = HashToken(rawRefreshToken),
            HetHan = now.Add(RefreshTokenLifetime)
        });

        // Dọn các token đã hết hạn của tài khoản này
        var expired = await _context.RefreshTokens
            .Where(t => t.TaiKhoanId == user.Id && t.HetHan < now)
            .ToListAsync();
        _context.RefreshTokens.RemoveRange(expired);

        await _context.SaveChangesAsync();

        return new TokenDto
        {
            AccessToken = _tokenService.GenerateAccessToken((int)user.Id, user.TenDangNhap, user.VaiTro, permissions: null),
            RefreshToken = rawRefreshToken,
            ExpiresIn = now.AddMinutes(15)
        };
    }

    private async Task RevokeAllAsync(long taiKhoanId, DateTime now)
    {
        await _context.RefreshTokens
            .Where(t => t.TaiKhoanId == taiKhoanId && t.ThuHoiLuc == null)
            .ExecuteUpdateAsync(s => s.SetProperty(t => t.ThuHoiLuc, (DateTime?)now));
    }

    /// <summary>Băm xác định (SHA-256) để tra cứu refresh token trong DB. Không dùng BCrypt vì mỗi lần băm ra giá trị khác nhau.</summary>
    private static string HashToken(string token) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token))).ToLowerInvariant();

    private static LoginResponseDto Fail(string message) => new()
    {
        Success = false,
        Message = message
    };
}
