using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RepairSystem.Service.DTOs;
using RepairSystem.Service.IServices;

namespace RepairSystem.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>Đăng nhập, trả về JWT access token + refresh token.</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<LoginResponseDto>>> Login([FromBody] LoginRequestDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<LoginResponseDto>.ErrorResponse("Dữ liệu không hợp lệ."));

        var result = await _authService.LoginAsync(request);

        if (!result.Success)
            return Unauthorized(ApiResponse<LoginResponseDto>.ErrorResponse(result.Message));

        return Ok(ApiResponse<LoginResponseDto>.SuccessResponse(result, result.Message));
    }

    /// <summary>Đăng ký tài khoản khách hàng mới (vai trò luôn là KHACH_HANG).</summary>
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<bool>>> Register([FromBody] RegisterRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            return BadRequest(ApiResponse<bool>.ErrorResponse("Dữ liệu không hợp lệ.", errors));
        }

        var (success, message) = await _authService.RegisterAsync(request);

        if (!success)
            return BadRequest(ApiResponse<bool>.ErrorResponse(message));

        return Ok(ApiResponse<bool>.SuccessResponse(true, message));
    }

    /// <summary>Làm mới access token bằng refresh token.</summary>
    [HttpPost("refresh-token")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<TokenDto>>> RefreshToken([FromBody] RefreshTokenRequestDto request)
    {
        try
        {
            var result = await _authService.RefreshTokenAsync(request.RefreshToken);
            return Ok(ApiResponse<TokenDto>.SuccessResponse(result, "Token đã được làm mới."));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ApiResponse<TokenDto>.ErrorResponse(ex.Message));
        }
    }

    /// <summary>Đăng xuất (thu hồi refresh token).</summary>
    [HttpPost("logout")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<bool>>> Logout()
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized(ApiResponse<bool>.ErrorResponse("Không xác định được người dùng."));

        await _authService.LogoutAsync(userId);
        return Ok(ApiResponse<bool>.SuccessResponse(true, "Đăng xuất thành công."));
    }

    /// <summary>Lấy thông tin tài khoản hiện tại.</summary>
    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<UserDto>>> GetCurrentUser()
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized(ApiResponse<UserDto>.ErrorResponse("Không xác định được người dùng."));

        var user = await _authService.GetUserByIdAsync(userId);
        if (user == null)
            return NotFound(ApiResponse<UserDto>.ErrorResponse("Không tìm thấy tài khoản."));

        return Ok(ApiResponse<UserDto>.SuccessResponse(user));
    }

    /// <summary>Đổi mật khẩu.</summary>
    [HttpPost("change-password")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<bool>>> ChangePassword([FromBody] ChangePasswordRequestDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<bool>.ErrorResponse("Dữ liệu không hợp lệ."));

        if (!TryGetUserId(out var userId))
            return Unauthorized(ApiResponse<bool>.ErrorResponse("Không xác định được người dùng."));

        var result = await _authService.ChangePasswordAsync(userId, request.OldPassword, request.NewPassword);
        if (!result)
            return BadRequest(ApiResponse<bool>.ErrorResponse("Mật khẩu cũ không đúng."));

        return Ok(ApiResponse<bool>.SuccessResponse(true, "Đổi mật khẩu thành công."));
    }

    private bool TryGetUserId(out long userId)
    {
        var value = User.FindFirst("UserId")?.Value
                    ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        return long.TryParse(value, out userId) && userId > 0;
    }
}
