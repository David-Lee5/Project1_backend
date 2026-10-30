using System.ComponentModel.DataAnnotations;

namespace RepairSystem.Service.DTOs;

public class ThietBiDto
{
    public long Id { get; set; }
    public string MaThietBi { get; set; } = null!;
    public long KhachHangId { get; set; }
    public string? MaKh { get; set; }
    public string TenThietBi { get; set; } = null!;
    public string? LoaiMay { get; set; }
    public string? SoSerial { get; set; }
    public DateOnly? NgayMua { get; set; }
    public string? GhiChu { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class UpsertThietBiDto
{
    [Range(1, long.MaxValue)]
    public long KhachHangId { get; set; }

    [Required, MaxLength(150)]
    public string TenThietBi { get; set; } = null!;

    [MaxLength(50)]
    public string? LoaiMay { get; set; }

    [MaxLength(100)]
    public string? SoSerial { get; set; }

    public DateOnly? NgayMua { get; set; }

    [MaxLength(4000)]
    public string? GhiChu { get; set; }
}
