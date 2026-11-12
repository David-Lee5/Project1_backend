using System;
using System.Collections.Generic;

namespace RepairSystem.Model.ScaffoldV3.Entities;

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

    public long? LinhKienSerialId { get; set; }

    public long? YeuCauChiTietId { get; set; }

    public virtual ICollection<BienDongKho> BienDongKhos { get; set; } = new List<BienDongKho>();

    public virtual ICollection<CamKetBaoHanh> CamKetBaoHanhs { get; set; } = new List<CamKetBaoHanh>();

    public virtual ICollection<ChiTietHoaDon> ChiTietHoaDons { get; set; } = new List<ChiTietHoaDon>();

    public virtual ChiTietYeuCauLinhKien? ChiTietYeuCauLinhKien { get; set; }

    public virtual LinhKienSerial? LinhKienSerial { get; set; }

    public virtual ICollection<LinhKienSerial> LinhKienSerials { get; set; } = new List<LinhKienSerial>();

    public virtual PhieuSuaChua Phieu { get; set; } = null!;

    public virtual ThuKho ThuKho { get; set; } = null!;

    public virtual TonKho TonKho { get; set; } = null!;

    public virtual ICollection<TraLinhKienKho> TraLinhKienKhos { get; set; } = new List<TraLinhKienKho>();
}
