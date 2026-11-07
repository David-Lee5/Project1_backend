using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using RepairSystem.Service.DTOs;
using RepairSystem.Service.Helpers;
using RepairSystem.Service.IServices;

namespace RepairSystem.Controllers.Auth;

[ApiController]
[Route("api/users")]
[Authorize(Roles = Roles.Admin)]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    /// <summary>Lấy danh sách tất cả tài khoản.</summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<UserDto>>>> GetAll()
    {
        var users = await _userService.GetAllUsersAsync();
        return Ok(ApiResponse<IEnumerable<UserDto>>.SuccessResponse(users));
    }

    /// <summary>Lấy thông tin tài khoản theo id.</summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<UserDto>>> GetById(int id)
    {
        var user = await _userService.GetUserByIdAsync(id);
        if (user == null)
            return NotFound(ApiResponse<UserDto>.ErrorResponse($"Không tìm thấy tài khoản id={id}."));

        return Ok(ApiResponse<UserDto>.SuccessResponse(user));
    }

    /// <summary>Lấy tài khoản theo tên đăng nhập.</summary>
    [HttpGet("by-username/{username}")]
    public async Task<ActionResult<ApiResponse<UserDto>>> GetByUsername(string username)
    {
        var user = await _userService.GetUserByUsernameAsync(username);
        if (user == null)
            return NotFound(ApiResponse<UserDto>.ErrorResponse($"Không tìm thấy tài khoản '{username}'."));

        return Ok(ApiResponse<UserDto>.SuccessResponse(user));
    }

    /// <summary>Cập nhật trạng thái hoạt động của tài khoản.</summary>
    [HttpPut("{id:int}/status")]
    public async Task<ActionResult<ApiResponse<bool>>> UpdateStatus(int id, [FromBody] UpdateUserStatusRequestDto request)
    {
        var result = await _userService.UpdateUserStatusAsync(id, request.IsActive);
        if (!result)
            return NotFound(ApiResponse<bool>.ErrorResponse($"Không tìm thấy tài khoản id={id}."));

        return Ok(ApiResponse<bool>.SuccessResponse(true, "Cập nhật trạng thái thành công."));
    }

    [HttpPost("{id:long}/reset-password")]
    public async Task<ActionResult<ApiResponse<bool>>> ResetPassword(long id, [FromBody] ResetPasswordRequest request)
    {
        var actorId = User.GetTaiKhoanId();
        if (actorId == null) return Unauthorized();
        if (actorId == id)
            return BadRequest(ApiResponse<bool>.ErrorResponse("Dùng chức năng đổi mật khẩu để thay mật khẩu của chính bạn."));
        var updated = await _userService.ResetPasswordAsync(id, request.NewPassword, actorId.Value);
        if (!updated) return NotFound(ApiResponse<bool>.ErrorResponse("Không tìm thấy tài khoản."));
        return Ok(ApiResponse<bool>.SuccessResponse(true, "Đã đặt lại mật khẩu. Các phiên làm mới đã bị thu hồi."));
    }

    /// <summary>Xoá tài khoản (soft-disable).</summary>
    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(int id)
    {
        var result = await _userService.DeleteUserAsync(id);
        if (!result)
            return NotFound(ApiResponse<bool>.ErrorResponse($"Không tìm thấy tài khoản id={id}."));

        return Ok(ApiResponse<bool>.SuccessResponse(true, "Xoá tài khoản thành công."));
    }
}

public sealed class ResetPasswordRequest
{
    [Required, StringLength(100, MinimumLength = 8)]
    public string NewPassword { get; set; } = null!;

    [Required, Compare(nameof(NewPassword))]
    public string ConfirmPassword { get; set; } = null!;
}
