using System;
using System.Collections.Generic;

namespace RepairSystem.Model.ScaffoldV3.Entities;

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

    public long? LoaiLinhKienId { get; set; }

    public string? MaModel { get; set; }

    public bool? TheoDoiSerial { get; set; }

    public virtual AnhLinhKien? AnhLinhKien { get; set; }

    public virtual ICollection<ChiTietBaoGium> ChiTietBaoGia { get; set; } = new List<ChiTietBaoGium>();

    public virtual ICollection<ChiTietNhapKho> ChiTietNhapKhos { get; set; } = new List<ChiTietNhapKho>();

    public virtual ICollection<ChiTietYeuCauLinhKien> ChiTietYeuCauLinhKiens { get; set; } = new List<ChiTietYeuCauLinhKien>();

    public virtual ICollection<LinhKienSerial> LinhKienSerials { get; set; } = new List<LinhKienSerial>();

    public virtual LoaiLinhKien? LoaiLinhKien { get; set; }

    public virtual ICollection<PhieuBaoHanhNcc> PhieuBaoHanhNccs { get; set; } = new List<PhieuBaoHanhNcc>();

    public virtual ICollection<ThongSoLinhKien> ThongSoLinhKiens { get; set; } = new List<ThongSoLinhKien>();

    public virtual ICollection<TonKho> TonKhos { get; set; } = new List<TonKho>();
}
