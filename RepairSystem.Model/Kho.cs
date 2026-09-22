using System;
using System.Collections.Generic;

namespace RepairSystem.Model;

public partial class Kho
{
    public string MaKho { get; set; } = null!;

    public string TenKho { get; set; } = null!;

    public string? DiaDiem { get; set; }

    public string? NguoiPhuTrach { get; set; }

    public virtual ICollection<CtXuatKho> CtXuatKhos { get; set; } = new List<CtXuatKho>();

    public virtual ICollection<PhieuNhap> PhieuNhaps { get; set; } = new List<PhieuNhap>();

    public virtual ICollection<TonKho> TonKhos { get; set; } = new List<TonKho>();
}
