using System;
using System.Collections.Generic;

namespace RepairSystem.Model;

public partial class CtNhapKho
{
    public string MaPn { get; set; } = null!;

    public string MaLk { get; set; } = null!;

    public int SoLuongNhap { get; set; }

    public decimal DonGiaNhap { get; set; }

    public decimal? ThanhTien { get; set; }

    public virtual LinhKien MaLkNavigation { get; set; } = null!;

    public virtual PhieuNhap MaPnNavigation { get; set; } = null!;
}
