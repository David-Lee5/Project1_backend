using System;
using System.Collections.Generic;

namespace RepairSystem.Model;

public partial class VTonKhoSapHet
{
    public long KhoId { get; set; }

    public string TenKho { get; set; } = null!;

    public long LinhKienId { get; set; }

    public string TenLinhKien { get; set; } = null!;

    public int SoLuongTon { get; set; }

    public int MucTonToiThieu { get; set; }
}
