using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RepairSystem.Service.DTOs;
using RepairSystem.Service.IServices;

namespace RepairSystem.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ITokenService _tokenService;

    public AuthController(IAuthService authService, ITokenService tokenService)
    {
        _authService = authService;
        _tokenService = tokenService;
    }

    /// <summary>
    /// Login user and return JWT tokens
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<LoginResponseDto>>> Login([FromBody] LoginRequestDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<LoginResponseDto>.ErrorResponse("Invalid input"));

        var result = await _authService.LoginAsync(request);

        if (!result.Success)
            return Unauthorized(result);

        return Ok(result);
    }

    /// <summary>
    /// Register a new user
    /// </summary>
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<bool>>> Register([FromBody] RegisterRequestDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<bool>.ErrorResponse("Invalid input"));

        var result = await _authService.RegisterAsync(request.Username, request.Email, request.Password, request.RoleId);

        if (!result)
            return BadRequest(ApiResponse<bool>.ErrorResponse("Username already exists"));

        return Ok(ApiResponse<bool>.SuccessResponse(true, "User registered successfully"));
    }

    /// <summary>
    /// Refresh access token using refresh token
    /// </summary>
    [HttpPost("refresh-token")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<TokenDto>>> RefreshToken([FromBody] RefreshTokenRequestDto request)
    {
        try
        {
            var result = await _authService.RefreshTokenAsync(request.RefreshToken);
            return Ok(ApiResponse<TokenDto>.SuccessResponse(result, "Token refreshed successfully"));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ApiResponse<TokenDto>.ErrorResponse(ex.Message));
        }
    }

    /// <summary>
    /// Logout current user
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<bool>>> Logout()
    {
        var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
        var result = await _authService.LogoutAsync(userId);

        if (!result)
            return BadRequest(ApiResponse<bool>.ErrorResponse("Logout failed"));

        return Ok(ApiResponse<bool>.SuccessResponse(true, "Logout successful"));
    }

    /// <summary>
    /// Get current user info
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<UserDto>>> GetCurrentUser()
    {
        var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
        var user = await _authService.GetUserByIdAsync(userId);

        if (user == null)
            return NotFound(ApiResponse<UserDto>.ErrorResponse("User not found"));

        return Ok(ApiResponse<UserDto>.SuccessResponse(user));
    }

    /// <summary>
    /// Change user password
    /// </summary>
    [HttpPost("change-password")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<bool>>> ChangePassword([FromBody] ChangePasswordRequestDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<bool>.ErrorResponse("Invalid input"));

        var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
        var result = await _authService.ChangePasswordAsync(userId, request.OldPassword, request.NewPassword);

        if (!result)
            return BadRequest(ApiResponse<bool>.ErrorResponse("Invalid password or password change failed"));

        return Ok(ApiResponse<bool>.SuccessResponse(true, "Password changed successfully"));
    }
}
