using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RepairSystem.Service.DTOs;
using RepairSystem.Service.Helpers;
using RepairSystem.Service.IServices;

namespace RepairSystem.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    /// <summary>
    /// Get all users
    /// </summary>
    [HttpGet]
    [RequirePermission("ViewUsers")]
    public async Task<ActionResult<ApiResponse<IEnumerable<UserDto>>>> GetAll()
    {
        var users = await _userService.GetAllUsersAsync();
        return Ok(ApiResponse<IEnumerable<UserDto>>.SuccessResponse(users));
    }

    /// <summary>
    /// Get user by id
    /// </summary>
    [HttpGet("{id}")]
    [RequirePermission("ViewUsers")]
    public async Task<ActionResult<ApiResponse<UserDto>>> GetById(int id)
    {
        var user = await _userService.GetUserByIdAsync(id);
        if (user == null)
            return NotFound(ApiResponse<UserDto>.ErrorResponse("User not found"));

        return Ok(ApiResponse<UserDto>.SuccessResponse(user));
    }

    /// <summary>
    /// Get user by username
    /// </summary>
    [HttpGet("username/{username}")]
    [RequirePermission("ViewUsers")]
    public async Task<ActionResult<ApiResponse<UserDto>>> GetByUsername(string username)
    {
        var user = await _userService.GetUserByUsernameAsync(username);
        if (user == null)
            return NotFound(ApiResponse<UserDto>.ErrorResponse("User not found"));

        return Ok(ApiResponse<UserDto>.SuccessResponse(user));
    }

    /// <summary>
    /// Create new user
    /// </summary>
    [HttpPost]
    [RequirePermission("CreateUser")]
    public async Task<ActionResult<ApiResponse<UserDto>>> Create([FromBody] CreateUserRequestDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<UserDto>.ErrorResponse("Invalid input"));

        try
        {
            var userDto = new UserDto
            {
                Username = request.Username,
                Email = request.Email,
                RoleId = request.RoleId
            };

            var user = await _userService.CreateUserAsync(userDto, request.Password);
            return CreatedAtAction(nameof(GetById), new { id = user.TaiKhoanId },
                ApiResponse<UserDto>.SuccessResponse(user, "User created successfully"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<UserDto>.ErrorResponse(ex.Message));
        }
    }

    /// <summary>
    /// Update existing user
    /// </summary>
    [HttpPut("{id}")]
    [RequirePermission("EditUser")]
    public async Task<ActionResult<ApiResponse<UserDto>>> Update(int id, [FromBody] UserDto userDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<UserDto>.ErrorResponse("Invalid input"));

        try
        {
            var user = await _userService.UpdateUserAsync(id, userDto);
            return Ok(ApiResponse<UserDto>.SuccessResponse(user, "User updated successfully"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<UserDto>.ErrorResponse(ex.Message));
        }
    }

    /// <summary>
    /// Delete user
    /// </summary>
    [HttpDelete("{id}")]
    [RequirePermission("DeleteUser")]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(int id)
    {
        var result = await _userService.DeleteUserAsync(id);
        if (!result)
            return NotFound(ApiResponse<bool>.ErrorResponse("User not found"));

        return Ok(ApiResponse<bool>.SuccessResponse(true, "User deleted successfully"));
    }

    /// <summary>
    /// Assign role to user
    /// </summary>
    [HttpPost("{id}/role/{roleId}")]
    [RequirePermission("EditUser")]
    public async Task<ActionResult<ApiResponse<bool>>> AssignRole(int id, int roleId)
    {
        var result = await _userService.AssignRoleToUserAsync(id, roleId);
        if (!result)
            return NotFound(ApiResponse<bool>.ErrorResponse("User or role not found"));

        return Ok(ApiResponse<bool>.SuccessResponse(true, "Role assigned successfully"));
    }

    /// <summary>
    /// Update user status
    /// </summary>
    [HttpPut("{id}/status")]
    [RequirePermission("EditUser")]
    public async Task<ActionResult<ApiResponse<bool>>> UpdateStatus(int id, [FromBody] UpdateUserStatusRequestDto request)
    {
        var result = await _userService.UpdateUserStatusAsync(id, request.IsActive);
        if (!result)
            return NotFound(ApiResponse<bool>.ErrorResponse("User not found"));

        return Ok(ApiResponse<bool>.SuccessResponse(true, "User status updated successfully"));
    }
}
