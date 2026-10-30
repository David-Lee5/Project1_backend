using System.ComponentModel.DataAnnotations;

namespace RepairSystem.Service.DTOs;

public class KhachHangDto
{
    public long Id { get; set; }
    public string MaKh { get; set; } = null!;
    public long? TaiKhoanId { get; set; }
    public string HoTen { get; set; } = null!;
    public string SoDienThoai { get; set; } = null!;
    public string? Email { get; set; }
    public string? DiaChi { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

/// <summary>Thông tin hồ sơ; tài khoản và vai trò không được thay đổi qua API này.</summary>
public class UpsertKhachHangDto
{
    [Required, MaxLength(100)]
    public string HoTen { get; set; } = null!;

    [Required, RegularExpression(@"^[0-9+ ]{8,15}$")]
    public string SoDienThoai { get; set; } = null!;

    [EmailAddress, MaxLength(255)]
    public string? Email { get; set; }

    [MaxLength(255)]
    public string? DiaChi { get; set; }
}

public class CustomerDevicePage<T>
{
    public IReadOnlyList<T> Items { get; set; } = Array.Empty<T>();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
}
