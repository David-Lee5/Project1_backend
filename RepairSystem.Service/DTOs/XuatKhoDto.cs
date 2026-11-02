using System.ComponentModel.DataAnnotations;

namespace RepairSystem.Service.DTOs;

/// <summary>Yêu cầu xuất linh kiện cho một phiếu sửa chữa.</summary>
public class XuatKhoRequestDto
{
    [Required]
    public long KhoId { get; set; }

    [Required]
    public long LinhKienId { get; set; }

    /// <summary>Số lượng xuất. Bị bỏ qua nếu có chọn serial (khi đó số lượng = số serial được chọn).</summary>
    public int SoLuong { get; set; } = 1;

    /// <summary>
    /// Các serial cụ thể được xuất (phải đang TRONG_KHO, đúng linh kiện, đúng kho).
    /// Bắt buộc nếu linh kiện đó trong kho đang được theo dõi theo serial.
    /// </summary>
    public List<long>? LinhKienSerialIds { get; set; }
}

/// <summary>Một dòng đã xuất kho cho phiếu sửa chữa.</summary>
public class DongXuatKhoDto
{
    public long ChiTietXuatKhoId { get; set; }
    public long PhieuId { get; set; }
    public long KhoId { get; set; }
    public string TenKho { get; set; } = null!;
    public long LinhKienId { get; set; }
    public string MaLk { get; set; } = null!;
    public string TenLinhKien { get; set; } = null!;
    public int SoLuongXuat { get; set; }
    public decimal DonGiaXuat { get; set; }
    public decimal ThanhTien { get; set; }
    public DateTime NgayXuat { get; set; }
    public DateOnly? BaoHanhDen { get; set; }

    /// <summary>Serial cụ thể (null nếu xuất theo số lượng, không theo dõi serial).</summary>
    public string? SoSerial { get; set; }
}
