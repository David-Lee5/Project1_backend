using System;
using System.Collections.Generic;

namespace RepairSystem.Model;

public partial class VLinhKienHienThi
{
    public long Id { get; set; }

    public string? MaLk { get; set; }

    public string TenLinhKien { get; set; } = null!;

    public string? HangSanXuat { get; set; }

    public string? MoTa { get; set; }

    public string DonViTinh { get; set; } = null!;

    public decimal DonGiaBan { get; set; }

    public short ThoiHanBaoHanhThang { get; set; }

    public string? AnhChinh { get; set; }

    public int? TongTon { get; set; }
}
