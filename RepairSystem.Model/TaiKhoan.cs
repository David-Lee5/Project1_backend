using System;
using System.Collections.Generic;

namespace RepairSystem.Model;

public partial class TaiKhoan
{
    public long Id { get; set; }

    public string TenDangNhap { get; set; } = null!;

    public string MatKhauHash { get; set; } = null!;

    public string VaiTro { get; set; } = null!;

    public bool HoatDong { get; set; }

    public short SoLanDangNhapSai { get; set; }

    public DateTime? KhoaDen { get; set; }

    public DateTime? DangNhapCuoi { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();

    public virtual ICollection<KhachHang> KhachHangs { get; set; } = new List<KhachHang>();

    public virtual ICollection<NhanVien> NhanViens { get; set; } = new List<NhanVien>();

    public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}
