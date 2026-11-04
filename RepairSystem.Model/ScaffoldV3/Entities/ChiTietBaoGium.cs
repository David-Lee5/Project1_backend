using System;
using System.Collections.Generic;

namespace RepairSystem.Model.ScaffoldV3.Entities;

public partial class ChiTietBaoGium
{
    public long Id { get; set; }

    public long BaoGiaId { get; set; }

    public string Loai { get; set; } = null!;

    public long? DichVuId { get; set; }

    public long? LinhKienId { get; set; }

    public string TenHangMuc { get; set; } = null!;

    public int SoLuong { get; set; }

    public decimal DonGia { get; set; }

    public decimal? ThanhTien { get; set; }

    public virtual BaoGium BaoGia { get; set; } = null!;

    public virtual DichVu? DichVu { get; set; }

    public virtual LinhKien? LinhKien { get; set; }
}
