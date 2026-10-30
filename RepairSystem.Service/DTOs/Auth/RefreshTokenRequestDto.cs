using System.ComponentModel.DataAnnotations;

namespace RepairSystem.Service.DTOs;

public class RefreshTokenRequestDto
{
    [Required(ErrorMessage = "Refresh token là bắt buộc.")]
    [StringLength(256)]
    public string RefreshToken { get; set; } = null!;
}
