using RepairSystem.Service.DTOs;

namespace RepairSystem.Service.IServices;

public interface IRoleService
{
    Task<IEnumerable<RoleDto>> GetAllRolesAsync();
    Task<RoleDto?> GetRoleByIdAsync(int roleId);
    Task<RoleDto> CreateRoleAsync(RoleDto roleDto);
    Task<RoleDto> UpdateRoleAsync(int roleId, RoleDto roleDto);
    Task<bool> DeleteRoleAsync(int roleId);
    Task<bool> AssignPermissionsToRoleAsync(int roleId, List<int> permissionIds);
}
