using System.ComponentModel.DataAnnotations;

namespace RepairSystem.Service.DTOs;

/// <summary>Đăng ký tài khoản khách hàng. Vai trò luôn là KHACH_HANG, không cho client chọn.</summary>
public class RegisterRequestDto
{
    [Required(ErrorMessage = "Tên đăng nhập là bắt buộc.")]
    [StringLength(50, MinimumLength = 4, ErrorMessage = "Tên đăng nhập dài 4–50 ký tự.")]
    [RegularExpression(@"^[a-zA-Z0-9_.]+$", ErrorMessage = "Tên đăng nhập chỉ gồm chữ, số, dấu chấm và gạch dưới.")]
    public string Username { get; set; } = null!;

    [Required(ErrorMessage = "Mật khẩu là bắt buộc.")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu dài tối thiểu 6 ký tự.")]
    public string Password { get; set; } = null!;

    [Required(ErrorMessage = "Vui lòng nhập lại mật khẩu.")]
    [Compare(nameof(Password), ErrorMessage = "Mật khẩu xác nhận không khớp.")]
    public string ConfirmPassword { get; set; } = null!;

    [Required(ErrorMessage = "Họ tên là bắt buộc.")]
    [StringLength(100)]
    public string HoTen { get; set; } = null!;

    /// <summary>8–15 ký tự gồm chữ số, dấu + hoặc khoảng trắng (khớp ràng buộc của DB).</summary>
    [Required(ErrorMessage = "Số điện thoại là bắt buộc.")]
    [RegularExpression(@"^[0-9+ ]{8,15}$", ErrorMessage = "Số điện thoại gồm 8–15 ký tự (chữ số, +, khoảng trắng).")]
    public string SoDienThoai { get; set; } = null!;

    [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
    [StringLength(255)]
    public string? Email { get; set; }
}
