using System;
using System.Collections.Generic;

namespace RepairSystem.Model;

public partial class TonKho
{
    public string MaKho { get; set; } = null!;

    public string MaLk { get; set; } = null!;

    public int SoLuongTon { get; set; }

    public DateTime? NgayCapNhat { get; set; }

    public virtual Kho MaKhoNavigation { get; set; } = null!;

    public virtual LinhKien MaLkNavigation { get; set; } = null!;
}
