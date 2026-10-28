using System.ComponentModel.DataAnnotations;

namespace RepairSystem.Service.DTOs;

/// <summary>Trả về thông tin nhân viên (đọc).</summary>
public class NhanVienDto
{
    public long Id { get; set; }
    public string MaNv { get; set; } = null!;
    public string HoTen { get; set; } = null!;
    public string SoDienThoai { get; set; } = null!;
    public string? Email { get; set; }
    public string ChucVu { get; set; } = null!;
    public long TaiKhoanId { get; set; }
    public string? TenDangNhap { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

/// <summary>Tạo nhân viên mới, đồng thời tạo tài khoản.</summary>
public class CreateNhanVienDto
{
    [Required] [MaxLength(100)]
    public string HoTen { get; set; } = null!;

    [Required] [MaxLength(20)]
    public string SoDienThoai { get; set; } = null!;

    [MaxLength(255)]
    public string? Email { get; set; }

    /// <summary>LE_TAN | KY_THUAT_VIEN | THU_KHO | ADMIN</summary>
    [Required]
    public string ChucVu { get; set; } = null!;

    // --- Thông tin tài khoản ---
    [Required] [MaxLength(50)]
    public string TenDangNhap { get; set; } = null!;

    [Required] [MinLength(6)]
    public string MatKhau { get; set; } = null!;
}

/// <summary>Cập nhật thông tin nhân viên (không đổi tài khoản).</summary>
public class UpdateNhanVienDto
{
    [Required] [MaxLength(100)]
    public string HoTen { get; set; } = null!;

    [Required] [MaxLength(20)]
    public string SoDienThoai { get; set; } = null!;

    [MaxLength(255)]
    public string? Email { get; set; }

    /// <summary>LE_TAN | KY_THUAT_VIEN | THU_KHO | ADMIN</summary>
    [Required]
    public string ChucVu { get; set; } = null!;
}
