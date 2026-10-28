using System;
using System.Collections.Generic;

namespace RepairSystem.Model;

public partial class ChiTietXuatKho
{
    public long Id { get; set; }

    public long PhieuId { get; set; }

    public long KhoId { get; set; }

    public long LinhKienId { get; set; }

    public long ThuKhoId { get; set; }

    public int SoLuongXuat { get; set; }

    public decimal DonGiaXuat { get; set; }

    public decimal? ThanhTien { get; set; }

    public DateTime NgayXuat { get; set; }

    public DateOnly? BaoHanhDen { get; set; }

    public virtual ICollection<BienDongKho> BienDongKhos { get; set; } = new List<BienDongKho>();

    public virtual PhieuSuaChua Phieu { get; set; } = null!;

    public virtual ThuKho ThuKho { get; set; } = null!;

    public virtual TonKho TonKho { get; set; } = null!;
}
