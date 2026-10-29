using RepairSystem.Service.DTOs;

namespace RepairSystem.Service.IServices;

public interface IUserService
{
    Task<IEnumerable<UserDto>> GetAllUsersAsync();
    Task<UserDto?> GetUserByIdAsync(int userId);
    Task<UserDto?> GetUserByUsernameAsync(string username);

    /// <summary>Vô hiệu hoá tài khoản (không xoá cứng vì còn ràng buộc khoá ngoại).</summary>
    Task<bool> DeleteUserAsync(int userId);

    /// <summary>Bật/tắt tài khoản. Bật lại sẽ gỡ khoá đăng nhập sai; tắt sẽ thu hồi mọi refresh token.</summary>
    Task<bool> UpdateUserStatusAsync(int userId, bool isActive);
}
