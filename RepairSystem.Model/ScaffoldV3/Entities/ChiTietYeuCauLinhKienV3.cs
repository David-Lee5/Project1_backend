using System;
using System.Collections.Generic;

namespace RepairSystem.Model.ScaffoldV3.Entities;

public partial class ChiTietYeuCauLinhKienV3
{
    public long Id { get; set; }

    public long YeuCauId { get; set; }

    public long PhieuId { get; set; }

    public long LinhKienId { get; set; }

    public int SoLuongYeuCau { get; set; }

    public int? SoLuongDuyet { get; set; }

    public string? GhiChu { get; set; }

    public virtual ICollection<ChiTietXuatKho> ChiTietXuatKhos { get; set; } = new List<ChiTietXuatKho>();

    public virtual LinhKien LinhKien { get; set; } = null!;

    public virtual YeuCauLinhKienV3 YeuCauLinhKienV3 { get; set; } = null!;
}
