using System;
using System.Collections.Generic;

namespace RepairSystem.Model;

public partial class LinhKienSerial
{
    public long Id { get; set; }

    public long LinhKienId { get; set; }

    public string SoSerial { get; set; } = null!;

    public long? KhoId { get; set; }

    public long PhieuNhapId { get; set; }

    public long? ChiTietXuatKhoId { get; set; }

    /// <summary>TRONG_KHO | DA_XUAT | BAO_HANH | THANH_LY</summary>
    public string TrangThai { get; set; } = "TRONG_KHO";

    public string? GhiChu { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ChiTietXuatKho? ChiTietXuatKho { get; set; }

    public virtual Kho? Kho { get; set; }

    public virtual LinhKien LinhKien { get; set; } = null!;

    public virtual PhieuNhap PhieuNhap { get; set; } = null!;
}
