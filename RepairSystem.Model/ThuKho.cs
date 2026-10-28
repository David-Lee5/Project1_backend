using System;
using System.Collections.Generic;

namespace RepairSystem.Model;

public partial class ThuKho
{
    public long NhanVienId { get; set; }

    public string ChucVu { get; set; } = null!;

    public virtual ICollection<ChiTietXuatKho> ChiTietXuatKhos { get; set; } = new List<ChiTietXuatKho>();

    public virtual ICollection<Kho> Khos { get; set; } = new List<Kho>();

    public virtual NhanVien NhanVien { get; set; } = null!;

    public virtual ICollection<PhieuNhap> PhieuNhaps { get; set; } = new List<PhieuNhap>();
}
