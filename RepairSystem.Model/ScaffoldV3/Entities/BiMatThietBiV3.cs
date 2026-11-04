using System;
using System.Collections.Generic;

namespace RepairSystem.Model.ScaffoldV3.Entities;

public partial class BiMatThietBiV3
{
    public long PhieuId { get; set; }

    public byte[] DuLieuMaHoa { get; set; } = null!;

    public string MaKhoa { get; set; } = null!;

    public DateTime NgayTao { get; set; }

    public DateTime HetHan { get; set; }

    public virtual PhieuSuaChua Phieu { get; set; } = null!;
}
