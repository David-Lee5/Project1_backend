using System.ComponentModel.DataAnnotations;

namespace RepairSystem.Service.DTOs;

public class ChangePasswordRequestDto
{
    [Required(ErrorMessage = "Mật khẩu cũ là bắt buộc.")]
    [StringLength(100)]
    public string OldPassword { get; set; } = null!;
    [Required(ErrorMessage = "Mật khẩu mới là bắt buộc.")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu dài tối thiểu 6 ký tự.")]
    public string NewPassword { get; set; } = null!;
    [Required(ErrorMessage = "Vui lòng nhập lại mật khẩu mới.")]
    [Compare(nameof(NewPassword), ErrorMessage = "Mật khẩu xác nhận không khớp.")]
    public string ConfirmPassword { get; set; } = null!;
}
