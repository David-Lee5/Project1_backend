using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RepairSystem.Service.DTOs;
using RepairSystem.Service.Helpers;
using RepairSystem.Service.IServices;

namespace RepairSystem.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RolesController : ControllerBase
{
    private readonly IRoleService _roleService;

    public RolesController(IRoleService roleService)
    {
        _roleService = roleService;
    }

    /// <summary>
    /// Get all roles
    /// </summary>
    [HttpGet]
    [RequirePermission("ViewRoles")]
    public async Task<ActionResult<ApiResponse<IEnumerable<RoleDto>>>> GetAll()
    {
        var roles = await _roleService.GetAllRolesAsync();
        return Ok(ApiResponse<IEnumerable<RoleDto>>.SuccessResponse(roles));
    }

    /// <summary>
    /// Get role by id
    /// </summary>
    [HttpGet("{id}")]
    [RequirePermission("ViewRoles")]
    public async Task<ActionResult<ApiResponse<RoleDto>>> GetById(int id)
    {
        var role = await _roleService.GetRoleByIdAsync(id);
        if (role == null)
            return NotFound(ApiResponse<RoleDto>.ErrorResponse("Role not found"));

        return Ok(ApiResponse<RoleDto>.SuccessResponse(role));
    }

    /// <summary>
    /// Create new role
    /// </summary>
    [HttpPost]
    [RequirePermission("CreateRole")]
    public async Task<ActionResult<ApiResponse<RoleDto>>> Create([FromBody] RoleDto roleDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<RoleDto>.ErrorResponse("Invalid input"));

        var role = await _roleService.CreateRoleAsync(roleDto);
        return CreatedAtAction(nameof(GetById), new { id = role.Id }, 
            ApiResponse<RoleDto>.SuccessResponse(role, "Role created successfully"));
    }

    /// <summary>
    /// Update existing role
    /// </summary>
    [HttpPut("{id}")]
    [RequirePermission("EditRole")]
    public async Task<ActionResult<ApiResponse<RoleDto>>> Update(int id, [FromBody] RoleDto roleDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<RoleDto>.ErrorResponse("Invalid input"));

        try
        {
            var role = await _roleService.UpdateRoleAsync(id, roleDto);
            return Ok(ApiResponse<RoleDto>.SuccessResponse(role, "Role updated successfully"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<RoleDto>.ErrorResponse(ex.Message));
        }
    }

    /// <summary>
    /// Delete role
    /// </summary>
    [HttpDelete("{id}")]
    [RequirePermission("DeleteRole")]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(int id)
    {
        var result = await _roleService.DeleteRoleAsync(id);
        if (!result)
            return NotFound(ApiResponse<bool>.ErrorResponse("Role not found"));

        return Ok(ApiResponse<bool>.SuccessResponse(true, "Role deleted successfully"));
    }

    /// <summary>
    /// Assign permissions to role
    /// </summary>
    [HttpPost("{id}/permissions")]
    [RequirePermission("EditRole")]
    public async Task<ActionResult<ApiResponse<bool>>> AssignPermissions(int id, [FromBody] List<int> permissionIds)
    {
        var result = await _roleService.AssignPermissionsToRoleAsync(id, permissionIds);
        if (!result)
            return NotFound(ApiResponse<bool>.ErrorResponse("Role not found"));

        return Ok(ApiResponse<bool>.SuccessResponse(true, "Permissions assigned successfully"));
    }
}
