using System;
using System.Collections.Generic;

namespace RepairSystem.Model;

public partial class TonKho
{
    public long KhoId { get; set; }

    public long LinhKienId { get; set; }

    public int SoLuongTon { get; set; }

    public int MucTonToiThieu { get; set; }

    public DateTime NgayCapNhat { get; set; }

    public virtual ICollection<BienDongKho> BienDongKhos { get; set; } = new List<BienDongKho>();

    public virtual ICollection<ChiTietXuatKho> ChiTietXuatKhos { get; set; } = new List<ChiTietXuatKho>();

    public virtual Kho Kho { get; set; } = null!;

    public virtual LinhKien LinhKien { get; set; } = null!;
}
