using System;
using System.Collections.Generic;

namespace RepairSystem.Model;

public partial class AnhLinhKien
{
    public long Id { get; set; }

    public long LinhKienId { get; set; }

    public string DuongDan { get; set; } = null!;

    public bool LaAnhChinh { get; set; }

    public short ThuTu { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual LinhKien LinhKien { get; set; } = null!;
}
