using System.Security.Claims;

namespace RepairSystem.Service.Helpers;

public static class ClaimsPrincipalExtensions
{
    /// <summary>Id tài khoản (tai_khoan.id) của người đang đăng nhập, lấy từ JWT. null nếu không xác định được.</summary>
    public static long? GetTaiKhoanId(this ClaimsPrincipal user)
    {
        var value = user.FindFirst("UserId")?.Value
                    ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        return long.TryParse(value, out var id) ? id : null;
    }
}
