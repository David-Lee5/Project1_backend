using System;
using System.Collections.Generic;

namespace RepairSystem.Model;

public partial class ChiTietDichVu
{
    public long PhieuId { get; set; }

    public long DichVuId { get; set; }

    public int SoLuong { get; set; }

    public decimal DonGiaThucTe { get; set; }

    public decimal? ThanhTien { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual DichVu DichVu { get; set; } = null!;

    public virtual PhieuSuaChua Phieu { get; set; } = null!;
}
