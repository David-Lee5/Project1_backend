using RepairSystem.Data;
using RepairSystem.Model;
using RepairSystem.Service.DTOs;
using RepairSystem.Service.IServices;
using Microsoft.EntityFrameworkCore;

namespace RepairSystem.Service.Services;

public class PermissionService : IPermissionService
{
    private readonly AppDbContext _context;

    public PermissionService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<PermissionDto>> GetAllPermissionsAsync()
    {
        var permissions = await _context.Set<Permission>().ToListAsync();
        return permissions.Select(MapToDto).ToList();
    }

    public async Task<PermissionDto?> GetPermissionByIdAsync(int permissionId)
    {
        var permission = await _context.Set<Permission>().FindAsync(permissionId);
        return permission == null ? null : MapToDto(permission);
    }

    public async Task<PermissionDto> CreatePermissionAsync(PermissionDto permissionDto)
    {
        var permission = new Permission
        {
            Name = permissionDto.Name,
            Description = permissionDto.Description,
            Module = permissionDto.Module,
            IsActive = permissionDto.IsActive
        };

        _context.Set<Permission>().Add(permission);
        await _context.SaveChangesAsync();

        return MapToDto(permission);
    }

    public async Task<PermissionDto> UpdatePermissionAsync(int permissionId, PermissionDto permissionDto)
    {
        var permission = await _context.Set<Permission>().FindAsync(permissionId);
        if (permission == null)
            throw new KeyNotFoundException($"Permission with id {permissionId} not found");

        permission.Name = permissionDto.Name;
        permission.Description = permissionDto.Description;
        permission.Module = permissionDto.Module;
        permission.IsActive = permissionDto.IsActive;

        await _context.SaveChangesAsync();

        return MapToDto(permission);
    }

    public async Task<bool> DeletePermissionAsync(int permissionId)
    {
        var permission = await _context.Set<Permission>().FindAsync(permissionId);
        if (permission == null)
            return false;

        _context.Set<Permission>().Remove(permission);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<IEnumerable<PermissionDto>> GetPermissionsByModuleAsync(string module)
    {
        var permissions = await _context.Set<Permission>()
            .Where(p => p.Module == module && p.IsActive)
            .ToListAsync();

        return permissions.Select(MapToDto).ToList();
    }

    public async Task<IEnumerable<PermissionDto>> GetRolePermissionsAsync(int roleId)
    {
        var role = await _context.Set<Role>()
            .Include(r => r.Permissions)
            .FirstOrDefaultAsync(r => r.Id == roleId);

        if (role == null)
            return new List<PermissionDto>();

        return role.Permissions.Select(MapToDto).ToList();
    }

    private PermissionDto MapToDto(Permission permission)
    {
        return new PermissionDto
        {
            Id = permission.Id,
            Name = permission.Name,
            Description = permission.Description,
            Module = permission.Module,
            IsActive = permission.IsActive
        };
    }
}
