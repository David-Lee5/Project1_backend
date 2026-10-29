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

    /// <summary>Chuyên môn (chỉ có với KY_THUAT_VIEN).</summary>
    public string? ChuyenMon { get; set; }

    public long TaiKhoanId { get; set; }
    public string? TenDangNhap { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

/// <summary>Tạo nhân viên mới: tạo tài khoản, nhân viên và dòng trong bảng con theo chức vụ (một transaction).</summary>
public class CreateNhanVienDto
{
    [Required] [MaxLength(100)]
    public string HoTen { get; set; } = null!;

    /// <summary>8–15 ký tự gồm chữ số, dấu + hoặc khoảng trắng (khớp ràng buộc của DB).</summary>
    [Required]
    [RegularExpression(@"^[0-9+ ]{8,15}$", ErrorMessage = "Số điện thoại gồm 8–15 ký tự (chữ số, +, khoảng trắng).")]
    public string SoDienThoai { get; set; } = null!;

    [EmailAddress] [MaxLength(255)]
    public string? Email { get; set; }

    /// <summary>LE_TAN | KY_THUAT_VIEN | THU_KHO | ADMIN</summary>
    [Required]
    public string ChucVu { get; set; } = null!;

    /// <summary>Chuyên môn, chỉ dùng khi ChucVu = KY_THUAT_VIEN (ví dụ: "Laptop", "Mainboard").</summary>
    [MaxLength(100)]
    public string? ChuyenMon { get; set; }

    // --- Thông tin tài khoản ---
    [Required] [StringLength(50, MinimumLength = 4)]
    [RegularExpression(@"^[a-zA-Z0-9_.]+$", ErrorMessage = "Tên đăng nhập chỉ gồm chữ, số, dấu chấm và gạch dưới.")]
    public string TenDangNhap { get; set; } = null!;

    [Required] [MinLength(6)]
    public string MatKhau { get; set; } = null!;
}

/// <summary>Cập nhật thông tin nhân viên (không đổi tài khoản, không đổi chức vụ).</summary>
public class UpdateNhanVienDto
{
    [Required] [MaxLength(100)]
    public string HoTen { get; set; } = null!;

    [Required]
    [RegularExpression(@"^[0-9+ ]{8,15}$", ErrorMessage = "Số điện thoại gồm 8–15 ký tự (chữ số, +, khoảng trắng).")]
    public string SoDienThoai { get; set; } = null!;

    [EmailAddress] [MaxLength(255)]
    public string? Email { get; set; }

    /// <summary>
    /// Tuỳ chọn. Hệ thống chưa hỗ trợ đổi chức vụ (ràng buộc khoá ngoại với tai_khoan và các bảng con),
    /// nên nếu gửi lên thì phải trùng chức vụ hiện tại.
    /// </summary>
    public string? ChucVu { get; set; }

    /// <summary>Chuyên môn, chỉ áp dụng khi nhân viên là KY_THUAT_VIEN.</summary>
    [MaxLength(100)]
    public string? ChuyenMon { get; set; }
}
