using System;
using System.Collections.Generic;

namespace RepairSystem.Model;

public partial class ChiTietNhapKho
{
    public long PhieuNhapId { get; set; }

    public long LinhKienId { get; set; }

    public int SoLuongNhap { get; set; }

    public decimal DonGiaNhap { get; set; }

    public decimal? ThanhTien { get; set; }

    /// <summary>JSON array các số serial đã quét khi nhập, ví dụ ["KG-1","KG-2"].</summary>
    public string? DanhSachSerial { get; set; }

    public virtual LinhKien LinhKien { get; set; } = null!;

    public virtual PhieuNhap PhieuNhap { get; set; } = null!;
}
