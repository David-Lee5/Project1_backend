using RepairSystem.Service.DTOs;

namespace RepairSystem.Service.IServices;

public interface IUserService
{
    Task<IEnumerable<UserDto>> GetAllUsersAsync();
    Task<UserDto?> GetUserByIdAsync(int userId);
    Task<UserDto?> GetUserByUsernameAsync(string username);
    Task<UserDto> CreateUserAsync(UserDto userDto, string password);
    Task<UserDto> UpdateUserAsync(int userId, UserDto userDto);
    Task<bool> DeleteUserAsync(int userId);
    Task<bool> AssignRoleToUserAsync(int userId, int roleId);
    Task<bool> UpdateUserStatusAsync(int userId, bool isActive);
}
