using System;
using System.Collections.Generic;

namespace RepairSystem.Model;

public partial class LinhKien
{
    public long Id { get; set; }

    public string MaLk { get; set; } = null!;

    public string TenLinhKien { get; set; } = null!;

    public string? HangSanXuat { get; set; }

    public string? MoTa { get; set; }

    public string DonViTinh { get; set; } = null!;

    public decimal DonGiaBan { get; set; }

    public short ThoiHanBaoHanhThang { get; set; }

    public DateTime? DeletedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<AnhLinhKien> AnhLinhKiens { get; set; } = new List<AnhLinhKien>();

    public virtual ICollection<ChiTietNhapKho> ChiTietNhapKhos { get; set; } = new List<ChiTietNhapKho>();

    public virtual ICollection<TonKho> TonKhos { get; set; } = new List<TonKho>();
}
