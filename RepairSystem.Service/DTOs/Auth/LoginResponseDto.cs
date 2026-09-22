namespace RepairSystem.Service.DTOs;

public class LoginResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = null!;
    public TokenDto? Token { get; set; }
    public UserDto? User { get; set; }
}
