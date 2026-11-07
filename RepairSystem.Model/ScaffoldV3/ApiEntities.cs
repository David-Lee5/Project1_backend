namespace RepairSystem.Model.ScaffoldV3.Entities;

public sealed class LichHenV3
{
    public long Id { get; set; }
    public long KhachHangId { get; set; }
    public DateTime ThoiGianMongMuon { get; set; }
    public string TenThietBi { get; set; } = null!;
    public string MoTaLoi { get; set; } = null!;
    public string TrangThai { get; set; } = null!;
    public string? GhiChu { get; set; }
    public long? PhieuId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public sealed class XuLyBaoHanhNccV3
{
    public long Id { get; set; }
    public long PhieuBaoHanhNccId { get; set; }
    public string Loai { get; set; } = null!;
    public long? KhoId { get; set; }
    public long? PhieuNhapId { get; set; }
    public int SoLuong { get; set; }
    public long NguoiThucHienId { get; set; }
    public DateTime ThoiGian { get; set; }
    public string? GhiChu { get; set; }
}

public partial class PhieuBaoHanhNcc
{
    public long? KhoGuiId { get; set; }
    public bool DaTruTon { get; set; }
    public string NguonHang { get; set; } = null!;
}

public partial class BaoGium
{
    public long? KhachHangPhanHoiId { get; set; }
}
