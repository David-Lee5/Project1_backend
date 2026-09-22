using System;
using System.Collections.Generic;

namespace RepairSystem.Model;

public partial class CtXuatKho
{
    public string MaPhieu { get; set; } = null!;

    public string MaLk { get; set; } = null!;

    public string? MaKho { get; set; }

    public int SoLuongXuat { get; set; }

    public decimal DonGiaXuat { get; set; }

    public decimal? ThanhTien { get; set; }

    public virtual Kho? MaKhoNavigation { get; set; }

    public virtual LinhKien MaLkNavigation { get; set; } = null!;

    public virtual PhieuSuaChua MaPhieuNavigation { get; set; } = null!;
}
