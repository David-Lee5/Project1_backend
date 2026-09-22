using System;
using System.Collections.Generic;

namespace RepairSystem.Model;

public partial class KhachHang
{
    public string MaKh { get; set; } = null!;

    public int? TaiKhoanId { get; set; }

    public string HoTen { get; set; } = null!;

    public string SoDienThoai { get; set; } = null!;

    public string? Email { get; set; }

    public string? DiaChi { get; set; }

    public virtual TaiKhoan? TaiKhoan { get; set; }

    public virtual ICollection<ThietBi> ThietBis { get; set; } = new List<ThietBi>();
}
