using System.ComponentModel.DataAnnotations;

namespace RepairSystem.Service.DTOs;

public class LoginRequestDto
{
    [Required(ErrorMessage = "Tên đăng nhập là bắt buộc.")]
    [StringLength(50)]
    public string Username { get; set; } = null!;
    [Required(ErrorMessage = "Mật khẩu là bắt buộc.")]
    [StringLength(100)]
    public string Password { get; set; } = null!;
}
