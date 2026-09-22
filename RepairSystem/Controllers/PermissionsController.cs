using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RepairSystem.Service.DTOs;
using RepairSystem.Service.Helpers;
using RepairSystem.Service.IServices;

namespace RepairSystem.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PermissionsController : ControllerBase
{
    private readonly IPermissionService _permissionService;

    public PermissionsController(IPermissionService permissionService)
    {
        _permissionService = permissionService;
    }

    /// <summary>
    /// Get all permissions
    /// </summary>
    [HttpGet]
    [RequirePermission("ViewPermissions")]
    public async Task<ActionResult<ApiResponse<IEnumerable<PermissionDto>>>> GetAll()
    {
        var permissions = await _permissionService.GetAllPermissionsAsync();
        return Ok(ApiResponse<IEnumerable<PermissionDto>>.SuccessResponse(permissions));
    }

    /// <summary>
    /// Get permission by id
    /// </summary>
    [HttpGet("{id}")]
    [RequirePermission("ViewPermissions")]
    public async Task<ActionResult<ApiResponse<PermissionDto>>> GetById(int id)
    {
        var permission = await _permissionService.GetPermissionByIdAsync(id);
        if (permission == null)
            return NotFound(ApiResponse<PermissionDto>.ErrorResponse("Permission not found"));

        return Ok(ApiResponse<PermissionDto>.SuccessResponse(permission));
    }

    /// <summary>
    /// Get permissions by module
    /// </summary>
    [HttpGet("module/{module}")]
    [RequirePermission("ViewPermissions")]
    public async Task<ActionResult<ApiResponse<IEnumerable<PermissionDto>>>> GetByModule(string module)
    {
        var permissions = await _permissionService.GetPermissionsByModuleAsync(module);
        return Ok(ApiResponse<IEnumerable<PermissionDto>>.SuccessResponse(permissions));
    }

    /// <summary>
    /// Get role permissions
    /// </summary>
    [HttpGet("role/{roleId}")]
    [RequirePermission("ViewPermissions")]
    public async Task<ActionResult<ApiResponse<IEnumerable<PermissionDto>>>> GetRolePermissions(int roleId)
    {
        var permissions = await _permissionService.GetRolePermissionsAsync(roleId);
        return Ok(ApiResponse<IEnumerable<PermissionDto>>.SuccessResponse(permissions));
    }

    /// <summary>
    /// Create new permission
    /// </summary>
    [HttpPost]
    [RequirePermission("CreatePermission")]
    public async Task<ActionResult<ApiResponse<PermissionDto>>> Create([FromBody] PermissionDto permissionDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<PermissionDto>.ErrorResponse("Invalid input"));

        var permission = await _permissionService.CreatePermissionAsync(permissionDto);
        return CreatedAtAction(nameof(GetById), new { id = permission.Id },
            ApiResponse<PermissionDto>.SuccessResponse(permission, "Permission created successfully"));
    }

    /// <summary>
    /// Update existing permission
    /// </summary>
    [HttpPut("{id}")]
    [RequirePermission("EditPermission")]
    public async Task<ActionResult<ApiResponse<PermissionDto>>> Update(int id, [FromBody] PermissionDto permissionDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<PermissionDto>.ErrorResponse("Invalid input"));

        try
        {
            var permission = await _permissionService.UpdatePermissionAsync(id, permissionDto);
            return Ok(ApiResponse<PermissionDto>.SuccessResponse(permission, "Permission updated successfully"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<PermissionDto>.ErrorResponse(ex.Message));
        }
    }

    /// <summary>
    /// Delete permission
    /// </summary>
    [HttpDelete("{id}")]
    [RequirePermission("DeletePermission")]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(int id)
    {
        var result = await _permissionService.DeletePermissionAsync(id);
        if (!result)
            return NotFound(ApiResponse<bool>.ErrorResponse("Permission not found"));

        return Ok(ApiResponse<bool>.SuccessResponse(true, "Permission deleted successfully"));
    }
}
