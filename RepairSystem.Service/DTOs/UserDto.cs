namespace RepairSystem.Service.DTOs;

public class UserDto
{
    public int TaiKhoanId { get; set; }
    public string Username { get; set; } = null!;
    public string Email { get; set; } = null!;
    public int? RoleId { get; set; }
    public string? RoleName { get; set; }
    public List<string> Permissions { get; set; } = new();
    public bool IsActive { get; set; }
}
