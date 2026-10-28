using System;
using System.Collections.Generic;

namespace RepairSystem.Model;

public partial class KhachHang
{
    public long Id { get; set; }

    public string MaKh { get; set; } = null!;

    public string HoTen { get; set; } = null!;

    public string SoDienThoai { get; set; } = null!;

    public string? Email { get; set; }

    public string? DiaChi { get; set; }

    public long? TaiKhoanId { get; set; }

    public string VaiTroTk { get; set; } = null!;

    public DateTime? DeletedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual TaiKhoan? TaiKhoan { get; set; }

    public virtual ICollection<ThietBi> ThietBis { get; set; } = new List<ThietBi>();
}
