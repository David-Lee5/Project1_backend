using System.ComponentModel.DataAnnotations;

namespace RepairSystem.Service.DTOs;

/// <summary>Trả về thông tin nhà cung cấp (đọc).</summary>
public class NhaCungCapDto
{
    public long Id { get; set; }
    public string MaNcc { get; set; } = null!;
    public string TenNcc { get; set; } = null!;
    public string? MaSoThue { get; set; }
    public string? SoDienThoai { get; set; }
    public string? Email { get; set; }
    public string? DiaChi { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

/// <summary>Tạo / cập nhật nhà cung cấp.</summary>
public class UpsertNhaCungCapDto
{
    [Required] [MaxLength(150)]
    public string TenNcc { get; set; } = null!;

    [MaxLength(20)]
    public string? MaSoThue { get; set; }

    [MaxLength(20)]
    public string? SoDienThoai { get; set; }

    [EmailAddress] [MaxLength(255)]
    public string? Email { get; set; }

    [MaxLength(255)]
    public string? DiaChi { get; set; }
}
