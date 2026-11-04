using System;
using System.Collections.Generic;

namespace RepairSystem.Model.ScaffoldV3.Entities;

public partial class VV3CongNoPhieu
{
    public long PhieuId { get; set; }

    public long? HoaDonId { get; set; }

    public decimal? TongThanhToan { get; set; }

    public decimal? DaThu { get; set; }

    public decimal? DaHoan { get; set; }

    public decimal? DaThuRong { get; set; }

    public decimal? ConNo { get; set; }
}
