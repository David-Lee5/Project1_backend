using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using RepairSystem.Model.ScaffoldV3.Entities;
using RepairSystem.Service.Helpers;

namespace RepairSystem.Controllers.V3;

internal static class V3Auth
{
    public static async Task<long?> NhanVienIdAsync(RepairDbContext db, ClaimsPrincipal user)
    {
        var taiKhoanId = user.GetTaiKhoanId();
        if (taiKhoanId == null) return null;
        return await db.NhanViens.AsNoTracking()
            .Where(x => x.TaiKhoanId == taiKhoanId && x.DeletedAt == null)
            .Select(x => (long?)x.Id).FirstOrDefaultAsync();
    }
}
