using System;
using System.Collections.Generic;

namespace RepairSystem.Model;

public partial class AuditLog
{
    public long Id { get; set; }

    public string Bang { get; set; } = null!;

    public long? BanGhiId { get; set; }

    public string HanhDong { get; set; } = null!;

    public string? DuLieuCu { get; set; }

    public string? DuLieuMoi { get; set; }

    public long? NguoiThucHienId { get; set; }

    public DateTime ThoiGian { get; set; }

    public virtual TaiKhoan? NguoiThucHien { get; set; }
}
