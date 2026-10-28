using System;
using System.Collections.Generic;

namespace RepairSystem.Model;

public partial class Kho
{
    public long Id { get; set; }

    public string MaKho { get; set; } = null!;

    public string TenKho { get; set; } = null!;

    public string? DiaDiem { get; set; }

    public long? ThuKhoId { get; set; }

    public DateTime? DeletedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<PhieuNhap> PhieuNhaps { get; set; } = new List<PhieuNhap>();

    public virtual ThuKho? ThuKho { get; set; }

    public virtual ICollection<TonKho> TonKhos { get; set; } = new List<TonKho>();
}
