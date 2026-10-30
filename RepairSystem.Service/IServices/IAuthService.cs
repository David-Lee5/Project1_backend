using RepairSystem.Service.DTOs;

namespace RepairSystem.Service.IServices;

public interface IAuthService
{
    Task<LoginResponseDto> LoginAsync(LoginRequestDto request);

    /// <summary>Đăng ký khách hàng: tạo tai_khoan (KHACH_HANG) và khach_hang trong một transaction.</summary>
    Task<(bool Success, string Message)> RegisterAsync(RegisterRequestDto request);

    Task<TokenDto> RefreshTokenAsync(string refreshToken);
    Task<bool> LogoutAsync(long userId);
    Task<UserDto?> GetUserByIdAsync(long userId);
    Task<bool> ChangePasswordAsync(long userId, string oldPassword, string newPassword);
}
