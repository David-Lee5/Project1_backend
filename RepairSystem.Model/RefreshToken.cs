using System;
using System.Collections.Generic;

namespace RepairSystem.Model;

public partial class RefreshToken
{
    public long Id { get; set; }

    public long TaiKhoanId { get; set; }

    public string TokenHash { get; set; } = null!;

    public DateTime HetHan { get; set; }

    public DateTime? ThuHoiLuc { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual TaiKhoan TaiKhoan { get; set; } = null!;
}
