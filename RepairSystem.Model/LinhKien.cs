using System;
using System.Collections.Generic;

namespace RepairSystem.Model;

public partial class LinhKien
{
    public string MaLk { get; set; } = null!;

    public string TenLinhKien { get; set; } = null!;

    public string DonViTinh { get; set; } = null!;

    public decimal DonGiaBan { get; set; }

    public virtual ICollection<CtNhapKho> CtNhapKhos { get; set; } = new List<CtNhapKho>();

    public virtual ICollection<CtXuatKho> CtXuatKhos { get; set; } = new List<CtXuatKho>();

    public virtual ICollection<TonKho> TonKhos { get; set; } = new List<TonKho>();
}
