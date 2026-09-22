using System;
using System.Collections.Generic;

namespace RepairSystem.Model;

public partial class DichVu
{
    public string MaDv { get; set; } = null!;

    public string TenDichVu { get; set; } = null!;

    public decimal GiaNiemYet { get; set; }

    public virtual ICollection<CtDichVu> CtDichVus { get; set; } = new List<CtDichVu>();
}
