namespace RepairSystem.Service.DTOs;

public class UserDto
{
    public long TaiKhoanId { get; set; }
    public string TenDangNhap { get; set; } = null!;
    public string VaiTro { get; set; } = null!;
    public bool HoatDong { get; set; }
}
