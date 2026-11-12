using System.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RepairSystem.Model.ScaffoldV3.Entities;
using RepairSystem.Service.DTOs;
using RepairSystem.Service.Helpers;

namespace RepairSystem.Controllers.V3;

[ApiController]
[Route("api/v3/phieu-sua-chua/{phieuId:long}/dich-vu")]
[Authorize(Roles = Roles.Admin + "," + Roles.LeTan + "," + Roles.KyThuatVien)]
public class DichVuPhieuController(RepairDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> DanhSach(long phieuId)
    {
        if (!await DuocXem(phieuId)) return Forbid();
        var rows = await db.ChiTietDichVus.AsNoTracking()
            .Where(x => x.PhieuId == phieuId)
            .Select(x => new { x.DichVuId, x.DichVu.TenDichVu, x.SoLuong,
                x.DonGiaThucTe, x.ThanhTien }).ToListAsync();
        return Ok(ApiResponse<object>.SuccessResponse(rows));
    }

    [HttpPost]
    public async Task<IActionResult> Them(long phieuId, [FromBody] ThemDichVuRequest req)
    {
        if (req.DichVuId <= 0 || req.SoLuong <= 0)
            return BadRequest(ApiResponse<object>.ErrorResponse("Dịch vụ hoặc số lượng không hợp lệ."));
        if (!await DuocXem(phieuId)) return Forbid();
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var phieu = await db.PhieuSuaChuas.AsNoTracking().FirstOrDefaultAsync(x => x.Id == phieuId);
        if (phieu == null || phieu.TrangThai is "HUY" or "DA_TRA")
            return Conflict(ApiResponse<object>.ErrorResponse("Phiếu đã kết thúc."));
        if (await db.HoaDons.AnyAsync(x => x.PhieuId == phieuId && x.TrangThai != "HUY"))
            return Conflict(ApiResponse<object>.ErrorResponse("Đã lập hóa đơn; không thể sửa hạng mục."));
        if (await db.ChiTietDichVus.AnyAsync(x => x.PhieuId == phieuId && x.DichVuId == req.DichVuId))
            return Conflict(ApiResponse<object>.ErrorResponse("Dịch vụ này đã có trong phiếu."));
        var quote = await db.BaoGia.AsNoTracking().Where(x => x.PhieuId == phieuId)
            .OrderByDescending(x => x.PhienBan).FirstOrDefaultAsync();
        if (quote == null || quote.TrangThai != "DA_DUYET")
            return Conflict(ApiResponse<object>.ErrorResponse("Cần báo giá mới nhất được khách duyệt."));
        var line = await db.ChiTietBaoGia.AsNoTracking().FirstOrDefaultAsync(x =>
            x.BaoGiaId == quote.Id && x.Loai == "DICH_VU" && x.DichVuId == req.DichVuId);
        if (line == null || req.SoLuong > line.SoLuong)
            return Conflict(ApiResponse<object>.ErrorResponse("Dịch vụ hoặc số lượng vượt báo giá đã duyệt."));
        db.ChiTietDichVus.Add(new ChiTietDichVu {
            PhieuId = phieuId, DichVuId = req.DichVuId,
            SoLuong = req.SoLuong, DonGiaThucTe = line.DonGia
        });
        await db.SaveChangesAsync();
        await tx.CommitAsync();
        return Ok(ApiResponse<object>.SuccessResponse(new { req.DichVuId, req.SoLuong,
            DonGia = line.DonGia }));
    }

    private async Task<bool> DuocXem(long phieuId)
    {
        if (!User.IsInRole(Roles.KyThuatVien)) return true;
        var actor = await NhanVienAuth.NhanVienIdAsync(db, User);
        return await db.PhieuSuaChuas.AnyAsync(x => x.Id == phieuId && x.KyThuatVienId == actor);
    }
}

public sealed record ThemDichVuRequest(long DichVuId, int SoLuong);
