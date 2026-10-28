using System;
using System.Collections.Generic;

namespace RepairSystem.Model;

public partial class DichVu
{
    public long Id { get; set; }

    public string MaDv { get; set; } = null!;

    public string TenDichVu { get; set; } = null!;

    public string? MoTa { get; set; }

    public string? HinhAnh { get; set; }

    public decimal GiaNiemYet { get; set; }

    public DateTime? DeletedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<ChiTietDichVu> ChiTietDichVus { get; set; } = new List<ChiTietDichVu>();
}
