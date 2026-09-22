using System;
using System.Collections.Generic;

namespace RepairSystem.Model;

public partial class PhieuNhap
{
    public string MaPn { get; set; } = null!;

    public DateTime? NgayNhap { get; set; }

    public decimal? TongTienNhap { get; set; }

    public string MaNcc { get; set; } = null!;

    public string MaKho { get; set; } = null!;

    public virtual ICollection<CtNhapKho> CtNhapKhos { get; set; } = new List<CtNhapKho>();

    public virtual Kho MaKhoNavigation { get; set; } = null!;

    public virtual NhaCungCap MaNccNavigation { get; set; } = null!;
}
