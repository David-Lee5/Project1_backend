using System;
using System.Collections.Generic;

namespace RepairSystem.Model.ScaffoldV3.Entities;

public partial class ChiTietHoaDonV3
{
    public long Id { get; set; }

    public long HoaDonId { get; set; }

    public long PhieuId { get; set; }

    public string Loai { get; set; } = null!;

    public long? DichVuId { get; set; }

    public long? ChiTietXuatKhoId { get; set; }

    public string TenHangMuc { get; set; } = null!;

    public int SoLuong { get; set; }

    public decimal DonGia { get; set; }

    public decimal? ThanhTien { get; set; }

    public virtual ChiTietDichVu? ChiTietDichVu { get; set; }

    public virtual ChiTietXuatKho? ChiTietXuatKho { get; set; }

    public virtual HoaDon HoaDon { get; set; } = null!;
}
