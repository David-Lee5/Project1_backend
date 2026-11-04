using System;
using System.Collections.Generic;

namespace RepairSystem.Model.ScaffoldV3.Entities;

public partial class TraLinhKienKhoV3
{
    public long Id { get; set; }

    public long ChiTietXuatKhoId { get; set; }

    public int SoLuong { get; set; }

    public long NguoiThucHienId { get; set; }

    public DateTime ThoiGian { get; set; }

    public string LyDo { get; set; } = null!;

    public virtual BienDongKho? BienDongKho { get; set; }

    public virtual ChiTietXuatKho ChiTietXuatKho { get; set; } = null!;

    public virtual ThuKho NguoiThucHien { get; set; } = null!;
}
