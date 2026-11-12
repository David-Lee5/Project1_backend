namespace RepairSystem.Model.ScaffoldV3.Entities;

public sealed class CongViecSuaChua
{
    public long Id { get; set; }
    public long PhieuId { get; set; }
    public long? PhieuBaoHanhId { get; set; }
    public string MoTa { get; set; } = null!;
    public long KyThuatVienId { get; set; }
    public string TrangThai { get; set; } = "CHO_LAM";
    public long TaoBoiId { get; set; }
    public DateTime TaoLuc { get; set; }
    public DateTime? BatDauLuc { get; set; }
    public DateTime? HoanThanhLuc { get; set; }
    public string? KetQua { get; set; }
    public decimal? CongDeXuat { get; set; }
    public long? CongDeXuatBoiId { get; set; }
    public decimal? CongDuyet { get; set; }
    public long? CongDuyetBoiId { get; set; }
    public DateTime? CongDuyetLuc { get; set; }
    public string? LyDoDuyetCong { get; set; }
    public DateTime? ChiTraLuc { get; set; }
    public long? ChiTraBoiId { get; set; }
    public string? MaThamChieuChiTra { get; set; }
    public string? PhuongThucChiTra { get; set; }
    public List<CongViecVatTu> VatTu { get; set; } = [];
}

public sealed class CongViecVatTu
{
    public long CongViecId { get; set; }
    public long ChiTietXuatKhoId { get; set; }
}

public sealed class BaoHanhPhanTich
{
    public long PhieuBaoHanhId { get; set; }
    public string NguyenNhan { get; set; } = null!;
    public string BenChiuChiPhi { get; set; } = null!;
    public decimal ChiPhiDuKien { get; set; }
    public decimal? ChiPhiThucTe { get; set; }
    public long NguoiGhiId { get; set; }
    public DateTime CapNhatLuc { get; set; }
}

public sealed class HoSoThanhLy
{
    public long Id { get; set; }
    public long CachLyId { get; set; }
    public string KetQuaKiemTra { get; set; } = null!;
    public string HuongXuLy { get; set; } = null!;
    public decimal GiaTriUocTinh { get; set; }
    public string TrangThai { get; set; } = null!;
    public long NguoiDeXuatId { get; set; }
    public DateTime DeXuatLuc { get; set; }
    public long? NguoiDuyetId { get; set; }
    public DateTime? DuyetLuc { get; set; }
    public string? LyDoQuyetDinh { get; set; }
    public decimal? GiaTriDuyet { get; set; }
    public decimal? GiaTriThuVe { get; set; }
}

public sealed class TaiLieuThanhLy
{
    public long Id { get; set; }
    public long HoSoId { get; set; }
    public string Loai { get; set; } = null!;
    public string TenGoc { get; set; } = null!;
    public string DuongDan { get; set; } = null!;
    public string Sha256 { get; set; } = null!;
    public long NguoiTaiId { get; set; }
    public DateTime TaiLuc { get; set; }
}

public sealed class HoaDonDauVao
{
    public long Id { get; set; }
    public long PhieuNhapId { get; set; }
    public string SoHoaDon { get; set; } = null!;
    public DateOnly NgayHoaDon { get; set; }
    public string MaSoThueNcc { get; set; } = null!;
    public decimal TienHang { get; set; }
    public decimal TienThue { get; set; }
    public decimal TongTien { get; set; }
    public string TrangThai { get; set; } = null!;
    public decimal ChenhLech { get; set; }
    public string? GhiChu { get; set; }
    public long NguoiGhiId { get; set; }
    public DateTime GhiLuc { get; set; }
}

public sealed class HoaDonDienTu
{
    public long Id { get; set; }
    public long HoaDonId { get; set; }
    public string NhaCungCap { get; set; } = null!;
    public string SoHoaDon { get; set; } = null!;
    public DateTime NgayPhatHanh { get; set; }
    public string? MaTraCuu { get; set; }
    public decimal TongTien { get; set; }
    public string TrangThai { get; set; } = "DA_GHI_NHAN";
    public long NguoiGhiId { get; set; }
    public DateTime GhiLuc { get; set; }
}

public sealed class DieuChinhHoaDon
{
    public long Id { get; set; }
    public long HoaDonDienTuId { get; set; }
    public string SoChungTu { get; set; } = null!;
    public string Loai { get; set; } = null!;
    public string LyDo { get; set; } = null!;
    public decimal ChenhLechTien { get; set; }
    public DateTime NgayChungTu { get; set; }
    public long NguoiGhiId { get; set; }
    public DateTime GhiLuc { get; set; }
}
