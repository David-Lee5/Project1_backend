using System;
using System.Collections.Generic;

namespace RepairSystem.Model;

public partial class AnhPhieuSuaChua
{
    public long Id { get; set; }

    public long PhieuId { get; set; }

    public string Loai { get; set; } = null!;

    public string DuongDan { get; set; } = null!;

    public string? MoTa { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual PhieuSuaChua Phieu { get; set; } = null!;
}
