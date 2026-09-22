using RepairSystem.Service.DTOs;

namespace RepairSystem.Service.IServices;

public interface IAuthService
{
    Task<LoginResponseDto> LoginAsync(LoginRequestDto request);
    Task<bool> RegisterAsync(string username, string email, string password, int? roleId = null);
    Task<TokenDto> RefreshTokenAsync(string refreshToken);
    Task<bool> LogoutAsync(int userId);
    Task<UserDto?> GetUserByIdAsync(int userId);
    Task<bool> ChangePasswordAsync(int userId, string oldPassword, string newPassword);
}
