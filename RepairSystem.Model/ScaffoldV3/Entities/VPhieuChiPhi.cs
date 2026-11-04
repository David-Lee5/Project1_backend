using System;
using System.Collections.Generic;

namespace RepairSystem.Model.ScaffoldV3.Entities;

public partial class VPhieuChiPhi
{
    public long PhieuId { get; set; }

    public decimal? TienDichVu { get; set; }

    public decimal? TienLinhKien { get; set; }

    public decimal? TongTamTinh { get; set; }
}
