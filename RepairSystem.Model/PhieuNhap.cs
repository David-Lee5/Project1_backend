using System;
using System.Collections.Generic;

namespace RepairSystem.Model;

public partial class PhieuNhap
{
    public long Id { get; set; }

    public string MaPn { get; set; } = null!;

    public long NhaCungCapId { get; set; }

    public long KhoId { get; set; }

    public long ThuKhoId { get; set; }

    public string? SoChungTuNcc { get; set; }

    public DateTime NgayNhap { get; set; }

    public string TrangThai { get; set; } = null!;

    public string? GhiChu { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<BienDongKho> BienDongKhos { get; set; } = new List<BienDongKho>();

    public virtual ICollection<ChiTietNhapKho> ChiTietNhapKhos { get; set; } = new List<ChiTietNhapKho>();

    public virtual Kho Kho { get; set; } = null!;

    public virtual NhaCungCap NhaCungCap { get; set; } = null!;

    public virtual ThuKho ThuKho { get; set; } = null!;
}
