using System.ComponentModel.DataAnnotations;

namespace RepairSystem.Service.DTOs;

/// <summary>Trả về thông tin dịch vụ (đọc).</summary>
public class DichVuDto
{
    public long Id { get; set; }
    public string MaDv { get; set; } = null!;
    public string TenDichVu { get; set; } = null!;
    public string? MoTa { get; set; }
    public string? HinhAnh { get; set; }
    public decimal GiaNiemYet { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

/// <summary>Tạo / cập nhật dịch vụ.</summary>
public class UpsertDichVuDto
{
    [Required] [MaxLength(150)]
    public string TenDichVu { get; set; } = null!;

    public string? MoTa { get; set; }

    [MaxLength(500)]
    public string? HinhAnh { get; set; }

    [Required] [Range(0, double.MaxValue)]
    public decimal GiaNiemYet { get; set; }
}
