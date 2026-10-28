using System;
using System.Collections.Generic;

namespace RepairSystem.Model;

public partial class NhaCungCap
{
    public long Id { get; set; }

    public string MaNcc { get; set; } = null!;

    public string TenNcc { get; set; } = null!;

    public string? MaSoThue { get; set; }

    public string? SoDienThoai { get; set; }

    public string? Email { get; set; }

    public string? DiaChi { get; set; }

    public DateTime? DeletedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<PhieuNhap> PhieuNhaps { get; set; } = new List<PhieuNhap>();
}
