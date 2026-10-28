using System;
using System.Collections.Generic;

namespace RepairSystem.Model;

public partial class NhanVien
{
    public long Id { get; set; }

    public string MaNv { get; set; } = null!;

    public string HoTen { get; set; } = null!;

    public string SoDienThoai { get; set; } = null!;

    public string? Email { get; set; }

    public string ChucVu { get; set; } = null!;

    public long TaiKhoanId { get; set; }

    public DateTime? DeletedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<BienDongKho> BienDongKhos { get; set; } = new List<BienDongKho>();

    public virtual ICollection<KyThuatVien> KyThuatViens { get; set; } = new List<KyThuatVien>();

    public virtual ICollection<LeTan> LeTans { get; set; } = new List<LeTan>();

    public virtual ICollection<LichSuTrangThaiPhieu> LichSuTrangThaiPhieus { get; set; } = new List<LichSuTrangThaiPhieu>();

    public virtual TaiKhoan TaiKhoan { get; set; } = null!;

    public virtual ICollection<ThuKho> ThuKhos { get; set; } = new List<ThuKho>();
}
