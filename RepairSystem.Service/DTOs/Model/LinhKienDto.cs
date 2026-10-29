using System.ComponentModel.DataAnnotations;

namespace RepairSystem.Service.DTOs;

/// <summary>Trả về thông tin linh kiện (đọc).</summary>
public class LinhKienDto
{
    public long Id { get; set; }
    public string MaLk { get; set; } = null!;
    public string TenLinhKien { get; set; } = null!;
    public string? HangSanXuat { get; set; }
    public string? MoTa { get; set; }
    public string DonViTinh { get; set; } = null!;

    /// <summary>Giá bán niêm yết (không phải giá vốn).</summary>
    public decimal DonGiaBan { get; set; }

    public int ThoiHanBaoHanhThang { get; set; }

    /// <summary>Tổng tồn của mọi kho. Chỉ trả về cho nhân viên, khách vãng lai thấy null.</summary>
    public int? TongTon { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

/// <summary>Tạo / cập nhật linh kiện.</summary>
public class UpsertLinhKienDto
{
    [Required] [MaxLength(150)]
    public string TenLinhKien { get; set; } = null!;

    [MaxLength(100)]
    public string? HangSanXuat { get; set; }

    public string? MoTa { get; set; }

    [Required] [MaxLength(20)]
    public string DonViTinh { get; set; } = "cái";

    [Required] [Range(0, 1_000_000_000_000.0, ErrorMessage = "Đơn giá bán phải lớn hơn hoặc bằng 0.")]
    public decimal DonGiaBan { get; set; }

    [Range(0, 120, ErrorMessage = "Thời hạn bảo hành từ 0 đến 120 tháng.")]
    public int ThoiHanBaoHanhThang { get; set; }
}
