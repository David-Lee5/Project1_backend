using RepairSystem.Data;
using RepairSystem.Model;
using RepairSystem.Service.DTOs;
using RepairSystem.Service.IServices;
using Microsoft.EntityFrameworkCore;

namespace RepairSystem.Service.Services;

public class RoleService : IRoleService
{
    private readonly AppDbContext _context;

    public RoleService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<RoleDto>> GetAllRolesAsync()
    {
        var roles = await _context.Set<Role>()
            .Include(r => r.Permissions)
            .ToListAsync();

        return roles.Select(MapToDto).ToList();
    }

    public async Task<RoleDto?> GetRoleByIdAsync(int roleId)
    {
        var role = await _context.Set<Role>()
            .Include(r => r.Permissions)
            .FirstOrDefaultAsync(r => r.Id == roleId);

        return role == null ? null : MapToDto(role);
    }

    public async Task<RoleDto> CreateRoleAsync(RoleDto roleDto)
    {
        var role = new Role
        {
            Name = roleDto.Name,
            Description = roleDto.Description,
            IsActive = roleDto.IsActive
        };

        _context.Set<Role>().Add(role);
        await _context.SaveChangesAsync();

        return MapToDto(role);
    }

    public async Task<RoleDto> UpdateRoleAsync(int roleId, RoleDto roleDto)
    {
        var role = await _context.Set<Role>().FindAsync(roleId);
        if (role == null)
            throw new KeyNotFoundException($"Role with id {roleId} not found");

        role.Name = roleDto.Name;
        role.Description = roleDto.Description;
        role.IsActive = roleDto.IsActive;
        role.UpdatedDate = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return MapToDto(role);
    }

    public async Task<bool> DeleteRoleAsync(int roleId)
    {
        var role = await _context.Set<Role>().FindAsync(roleId);
        if (role == null)
            return false;

        _context.Set<Role>().Remove(role);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> AssignPermissionsToRoleAsync(int roleId, List<int> permissionIds)
    {
        var role = await _context.Set<Role>()
            .Include(r => r.Permissions)
            .FirstOrDefaultAsync(r => r.Id == roleId);

        if (role == null)
            return false;

        var permissions = await _context.Set<Permission>()
            .Where(p => permissionIds.Contains(p.Id))
            .ToListAsync();

        role.Permissions = permissions;
        role.UpdatedDate = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return true;
    }

    private RoleDto MapToDto(Role role)
    {
        return new RoleDto
        {
            Id = role.Id,
            Name = role.Name,
            Description = role.Description,
            IsActive = role.IsActive,
            Permissions = role.Permissions?.Select(p => new PermissionDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Module = p.Module,
                IsActive = p.IsActive
            }).ToList() ?? new List<PermissionDto>()
        };
    }
}
