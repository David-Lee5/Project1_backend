using System;
using System.Collections.Generic;

namespace RepairSystem.Model.ScaffoldV3.Entities;

public partial class LinhKienSerial
{
    public long Id { get; set; }

    public long LinhKienId { get; set; }

    public string SoSerial { get; set; } = null!;

    public long? KhoId { get; set; }

    public long PhieuNhapId { get; set; }

    public long? ChiTietXuatKhoId { get; set; }

    public string TrangThai { get; set; } = null!;

    public string? GhiChu { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ChiTietXuatKho? ChiTietXuatKho { get; set; }

    public virtual ICollection<ChiTietXuatKho> ChiTietXuatKhos { get; set; } = new List<ChiTietXuatKho>();

    public virtual Kho? Kho { get; set; }

    public virtual LinhKien LinhKien { get; set; } = null!;

    public virtual ICollection<PhieuBaoHanhNcc> PhieuBaoHanhNccs { get; set; } = new List<PhieuBaoHanhNcc>();

    public virtual PhieuNhap PhieuNhap { get; set; } = null!;
}
