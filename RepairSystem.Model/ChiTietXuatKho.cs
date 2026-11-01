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

    public long? LinhKienSerialId { get; set; }

    public virtual ICollection<BienDongKho> BienDongKhos { get; set; } = new List<BienDongKho>();

    /// <summary>Serial cụ thể được gắn vào dòng xuất này (qua cột linh_kien_serial_id).</summary>
    public virtual LinhKienSerial? LinhKienSerial { get; set; }

    /// <summary>Các serial có chi_tiet_xuat_kho_id trỏ về dòng xuất này.</summary>
    public virtual ICollection<LinhKienSerial> LinhKienSerialsXuat { get; set; } = new List<LinhKienSerial>();

    public virtual PhieuSuaChua Phieu { get; set; } = null!;

    public virtual ThuKho ThuKho { get; set; } = null!;

    public virtual TonKho TonKho { get; set; } = null!;
}
