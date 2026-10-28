using System;
using System.Collections.Generic;

namespace RepairSystem.Model;

public partial class BienDongKho
{
    public long Id { get; set; }

    public long KhoId { get; set; }

    public long LinhKienId { get; set; }

    public string Loai { get; set; } = null!;

    public int SoLuongThayDoi { get; set; }

    public int SoLuongSau { get; set; }

    public long? PhieuNhapId { get; set; }

    public long? ChiTietXuatKhoId { get; set; }

    public long NguoiThucHienId { get; set; }

    public DateTime ThoiGian { get; set; }

    public string? LyDo { get; set; }

    public virtual ChiTietXuatKho? ChiTietXuatKho { get; set; }

    public virtual NhanVien NguoiThucHien { get; set; } = null!;

    public virtual PhieuNhap? PhieuNhap { get; set; }

    public virtual TonKho TonKho { get; set; } = null!;
}
