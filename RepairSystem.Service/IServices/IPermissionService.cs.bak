using RepairSystem.Service.DTOs;

namespace RepairSystem.Service.IServices;

public interface IPermissionService
{
    Task<IEnumerable<PermissionDto>> GetAllPermissionsAsync();
    Task<PermissionDto?> GetPermissionByIdAsync(int permissionId);
    Task<PermissionDto> CreatePermissionAsync(PermissionDto permissionDto);
    Task<PermissionDto> UpdatePermissionAsync(int permissionId, PermissionDto permissionDto);
    Task<bool> DeletePermissionAsync(int permissionId);
    Task<IEnumerable<PermissionDto>> GetPermissionsByModuleAsync(string module);
    Task<IEnumerable<PermissionDto>> GetRolePermissionsAsync(int roleId);
}
