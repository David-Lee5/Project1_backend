using System;
using System.Collections.Generic;

namespace RepairSystem.Model;

public partial class CtDichVu
{
    public string MaPhieu { get; set; } = null!;

    public string MaDv { get; set; } = null!;

    public int SoLuong { get; set; }

    public decimal DonGiaThucTe { get; set; }

    public decimal? ThanhTien { get; set; }

    public virtual DichVu MaDvNavigation { get; set; } = null!;

    public virtual PhieuSuaChua MaPhieuNavigation { get; set; } = null!;
}
