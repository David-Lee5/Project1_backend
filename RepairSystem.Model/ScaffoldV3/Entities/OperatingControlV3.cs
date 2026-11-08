namespace RepairSystem.Model.ScaffoldV3.Entities;

public sealed class QuyTacVanHanhV3
{
    public byte Id { get; set; }
    public byte BaoHanhLapDuyetTuLan { get; set; }
    public byte AnhTiepNhanToiThieu { get; set; }
    public string QuyTacTraCong { get; set; } = null!;
    public string ChinhSachSerial { get; set; } = null!;
    public string? LoaiHinhKinhDoanh { get; set; }
    public string? PhuongPhapThue { get; set; }
    public string? DonViHoaDonDienTu { get; set; }
    public long? NguoiCapNhatId { get; set; }
    public DateTime CapNhatLuc { get; set; }
}

public sealed class XacNhanTiepNhanV3
{
    public long PhieuId { get; set; }
    public string NguoiXacNhan { get; set; } = null!;
    public long AnhBienNhanId { get; set; }
    public long NhanVienGhiNhanId { get; set; }
    public DateTime XacNhanLuc { get; set; }
}

public sealed class DuyetBaoHanhLapV3
{
    public long PhieuBaoHanhId { get; set; }
    public int LanThu { get; set; }
    public string TrangThai { get; set; } = null!;
    public long NguoiDeXuatId { get; set; }
    public long? NguoiDuyetId { get; set; }
    public string? NguyenNhanSoBo { get; set; }
    public string? LyDoQuyetDinh { get; set; }
    public DateTime? NgayDuyet { get; set; }
}

public sealed class KiemKeKhoV3
{
    public long Id { get; set; }
    public long KhoId { get; set; }
    public string TrangThai { get; set; } = null!;
    public long NguoiKiemId { get; set; }
    public long? NguoiDuyetId { get; set; }
    public DateTime TaoLuc { get; set; }
    public DateTime? DuyetLuc { get; set; }
    public string? GhiChu { get; set; }
    public string? LyDoQuyetDinh { get; set; }
    public List<DongKiemKeKhoV3> ChiTiet { get; set; } = [];
}

public sealed class DongKiemKeKhoV3
{
    public long KiemKeId { get; set; }
    public long LinhKienId { get; set; }
    public int SoLuongSoSach { get; set; }
    public int SoLuongThucTe { get; set; }
    public string? SerialSoSachJson { get; set; }
    public string? SerialThucTeJson { get; set; }
}

public sealed class BienDongSerialKiemKeV3
{
    public long Id { get; set; }
    public long KiemKeId { get; set; }
    public long LinhKienSerialId { get; set; }
    public string TrangThaiCu { get; set; } = null!;
    public string TrangThaiMoi { get; set; } = null!;
    public DateTime ThoiGian { get; set; }
}

public sealed class CachLyVatTuV3
{
    public long Id { get; set; }
    public long KhoId { get; set; }
    public long LinhKienId { get; set; }
    public long? LinhKienSerialId { get; set; }
    public int SoLuong { get; set; }
    public string TrangThai { get; set; } = null!;
    public string LyDo { get; set; } = null!;
    public long NguoiTaoId { get; set; }
    public DateTime TaoLuc { get; set; }
    public long? NguoiXuLyId { get; set; }
    public DateTime? XuLyLuc { get; set; }
    public string? LyDoXuLy { get; set; }
}
