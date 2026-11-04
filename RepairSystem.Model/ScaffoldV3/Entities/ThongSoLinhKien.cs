using System;
using System.Collections.Generic;

namespace RepairSystem.Model.ScaffoldV3.Entities;

public partial class ThongSoLinhKien
{
    public long LinhKienId { get; set; }

    public long ThuocTinhId { get; set; }

    public string? GiaTriVanBan { get; set; }

    public decimal? GiaTriSo { get; set; }

    public bool? GiaTriDungSai { get; set; }

    public long? LuaChonId { get; set; }

    public virtual LinhKien LinhKien { get; set; } = null!;

    public virtual LuaChonThuocTinhLinhKien? LuaChonThuocTinhLinhKien { get; set; }

    public virtual ThuocTinhLoaiLinhKien ThuocTinh { get; set; } = null!;
}
