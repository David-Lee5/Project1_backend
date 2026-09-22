using System;
using System.Collections.Generic;

namespace RepairSystem.Model;

public partial class TaiKhoan
{
    public int TaiKhoanId { get; set; }

    public string Username { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public int? RoleId { get; set; }

    public bool? TrangThaiHoatDong { get; set; } = true;

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedDate { get; set; }

    public string? RefreshToken { get; set; }

    public DateTime? RefreshTokenExpiryTime { get; set; }

    public virtual Role? Role { get; set; }

    public virtual KhachHang? KhachHang { get; set; }

    public virtual KyThuatVien? KyThuatVien { get; set; }
}
