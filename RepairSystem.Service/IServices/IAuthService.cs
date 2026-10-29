using RepairSystem.Service.DTOs;

namespace RepairSystem.Service.IServices;

public interface IAuthService
{
    Task<LoginResponseDto> LoginAsync(LoginRequestDto request);

    /// <summary>Đăng ký khách hàng: tạo tai_khoan (KHACH_HANG) và khach_hang trong một transaction.</summary>
    Task<(bool Success, string Message)> RegisterAsync(RegisterRequestDto request);

    Task<TokenDto> RefreshTokenAsync(string refreshToken);
    Task<bool> LogoutAsync(int userId);
    Task<UserDto?> GetUserByIdAsync(int userId);
    Task<bool> ChangePasswordAsync(int userId, string oldPassword, string newPassword);
}
