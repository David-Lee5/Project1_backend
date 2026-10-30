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
    public string? HinhAnh { get; set; }
    public string DonViTinh { get; set; } = null!;

    /// <summary>Giá bán niêm yết (không phải giá vốn).</summary>
    public decimal DonGiaBan { get; set; }

    public int ThoiHanBaoHanhThang { get; set; }

    /// <summary>Tổng tồn của mọi kho. Chỉ trả về cho nhân viên, khách vãng lai thấy null.</summary>
    public long? TongTon { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

/// <summary>Metadata ảnh đã có URL; hệ thống không tải nội dung từ URL này.</summary>
public class AnhLinhKienDto
{
    public long Id { get; set; }
    public long LinhKienId { get; set; }
    public string DuongDan { get; set; } = null!;
    public bool LaAnhChinh { get; set; }
    public short ThuTu { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateAnhLinhKienDto
{
    [Required] [MaxLength(500)]
    public string DuongDan { get; set; } = null!;
    public bool LaAnhChinh { get; set; }
    [Range(0, short.MaxValue)]
    public int ThuTu { get; set; }
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
