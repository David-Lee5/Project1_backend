using System.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RepairSystem.Model.ScaffoldV3.Entities;
using RepairSystem.Service.DTOs;
using RepairSystem.Service.Helpers;

namespace RepairSystem.Controllers.V3;

[ApiController]
[Route("api/v3/phieu-sua-chua/{phieuId:long}/bao-gia")]
[Authorize(Roles = Roles.Admin + "," + Roles.LeTan)]
public class BaoGiaV3Controller(AppDbContextV3 db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> DanhSach(long phieuId)
    {
        if (!await db.PhieuSuaChuas.AnyAsync(x => x.Id == phieuId)) return NotFound();
        var rows = await db.BaoGia.AsNoTracking().Where(x => x.PhieuId == phieuId)
            .Include(x => x.ChiTietBaoGia).OrderByDescending(x => x.PhienBan).ToListAsync();
        return Ok(ApiResponse<object>.SuccessResponse(rows.Select(x => new {
            x.Id, x.PhienBan, x.TrangThai, x.GiamGia, x.NgayTao, x.NgayGui,
            x.NgayPhanHoi, x.KenhPhanHoi, x.NoiDungPhanHoi,
            TongTruocGiam = x.ChiTietBaoGia.Sum(c => c.SoLuong * c.DonGia),
            TongSauGiam = x.ChiTietBaoGia.Sum(c => c.SoLuong * c.DonGia) - x.GiamGia,
            ChiTiet = x.ChiTietBaoGia.Select(c => new {
                c.Id, c.Loai, c.DichVuId, c.LinhKienId, c.TenHangMuc,
                c.SoLuong, c.DonGia, ThanhTien = c.SoLuong * c.DonGia
            })
        }).ToList()));
    }

    [HttpPost]
    public async Task<IActionResult> Tao(long phieuId, [FromBody] TaoBaoGiaRequest req)
    {
        if (req.ChiTiet == null || req.ChiTiet.Count == 0 || req.GiamGia < 0
            || req.ChiTiet.Any(c => c.SoLuong <= 0 || c.DonGia < 0
                || c.DonGia > 999_999_999_999.99m || string.IsNullOrWhiteSpace(c.TenHangMuc)
                || c.TenHangMuc.Length > 250
                || !((c.Loai == "DICH_VU" && c.DichVuId > 0 && c.LinhKienId == null)
                     || (c.Loai == "LINH_KIEN" && c.LinhKienId > 0 && c.DichVuId == null)
                     || (c.Loai == "KHAC" && c.DichVuId == null && c.LinhKienId == null))))
            return BadRequest(ApiResponse<object>.ErrorResponse("Chi tiết báo giá không hợp lệ."));
        decimal tong;
        try { tong = req.ChiTiet.Sum(c => checked(c.SoLuong * c.DonGia)); }
        catch (OverflowException) { return BadRequest(ApiResponse<object>.ErrorResponse("Tổng báo giá quá lớn.")); }
        if (req.GiamGia > tong) return BadRequest(ApiResponse<object>.ErrorResponse("Giảm giá vượt tổng báo giá."));
        var actor = await V3Auth.NhanVienIdAsync(db, User);
        if (actor == null) return Unauthorized();
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var phieu = await db.PhieuSuaChuas.FirstOrDefaultAsync(x => x.Id == phieuId);
        if (phieu == null) return NotFound();
        if (phieu.TrangThai is "HUY" or "DA_TRA")
            return Conflict(ApiResponse<object>.ErrorResponse("Phiếu sửa chữa đã kết thúc."));
        var phienBan = (await db.BaoGia.Where(x => x.PhieuId == phieuId)
            .MaxAsync(x => (int?)x.PhienBan) ?? 0) + 1;
        var olderOpen = await db.BaoGia.Where(x => x.PhieuId == phieuId
            && (x.TrangThai == "NHAP" || x.TrangThai == "CHO_DUYET")).ToListAsync();
        foreach (var old in olderOpen) old.TrangThai = "HUY";
        var row = new BaoGium {
            PhieuId = phieuId, PhienBan = phienBan, TrangThai = "NHAP",
            GiamGia = req.GiamGia, NguoiTaoId = actor.Value
        };
        db.BaoGia.Add(row);
        foreach (var c in req.ChiTiet)
            row.ChiTietBaoGia.Add(new ChiTietBaoGium {
                Loai = c.Loai, DichVuId = c.DichVuId, LinhKienId = c.LinhKienId,
                TenHangMuc = c.TenHangMuc!.Trim(), SoLuong = c.SoLuong, DonGia = c.DonGia
            });
        await db.SaveChangesAsync();
        await tx.CommitAsync();
        return Ok(ApiResponse<object>.SuccessResponse(new { row.Id, row.PhienBan, row.TrangThai }));
    }

    [HttpPost("{baoGiaId:long}/gui")]
    public async Task<IActionResult> Gui(long phieuId, long baoGiaId)
    {
        var row = await db.BaoGia.FirstOrDefaultAsync(x => x.Id == baoGiaId && x.PhieuId == phieuId);
        if (row == null) return NotFound();
        if (row.TrangThai != "NHAP") return Conflict(ApiResponse<object>.ErrorResponse("Chỉ gửi được báo giá nháp."));
        if (row.PhienBan != await db.BaoGia.Where(x => x.PhieuId == phieuId).MaxAsync(x => x.PhienBan))
            return Conflict(ApiResponse<object>.ErrorResponse("Chỉ gửi phiên bản báo giá mới nhất."));
        row.NgayGui = DateTime.UtcNow;
        row.TrangThai = "CHO_DUYET";
        try { await db.SaveChangesAsync(); }
        catch (DbUpdateException) { return Conflict(ApiResponse<object>.ErrorResponse("Báo giá phải có chi tiết hợp lệ trước khi gửi.")); }
        return Ok(ApiResponse<object>.SuccessResponse(new { row.Id, row.TrangThai, row.NgayGui }));
    }

    [HttpPost("{baoGiaId:long}/phan-hoi")]
    public async Task<IActionResult> PhanHoi(long phieuId, long baoGiaId, [FromBody] PhanHoiBaoGiaRequest req)
    {
        if (req.QuyetDinh is not ("DA_DUYET" or "TU_CHOI")
            || string.IsNullOrWhiteSpace(req.KenhPhanHoi) || req.KenhPhanHoi.Length > 100
            || req.NoiDungPhanHoi?.Length > 1000)
            return BadRequest(ApiResponse<object>.ErrorResponse("Phản hồi không hợp lệ."));
        var actor = await V3Auth.NhanVienIdAsync(db, User);
        if (actor == null) return Unauthorized();
        var row = await db.BaoGia.FirstOrDefaultAsync(x => x.Id == baoGiaId && x.PhieuId == phieuId);
        if (row == null) return NotFound();
        if (row.TrangThai != "CHO_DUYET") return Conflict(ApiResponse<object>.ErrorResponse("Báo giá không còn chờ phản hồi."));
        if (row.PhienBan != await db.BaoGia.Where(x => x.PhieuId == phieuId).MaxAsync(x => x.PhienBan))
            return Conflict(ApiResponse<object>.ErrorResponse("Chỉ phản hồi phiên bản báo giá mới nhất."));
        row.TrangThai = req.QuyetDinh;
        row.NgayPhanHoi = DateTime.UtcNow;
        row.KenhPhanHoi = req.KenhPhanHoi.Trim();
        row.NguoiGhiNhanId = actor;
        row.NoiDungPhanHoi = req.NoiDungPhanHoi?.Trim();
        await db.SaveChangesAsync();
        return Ok(ApiResponse<object>.SuccessResponse(new { row.Id, row.TrangThai, row.NgayPhanHoi }));
    }
}

public sealed record TaoBaoGiaRequest(decimal GiamGia, List<DongBaoGiaRequest>? ChiTiet);
public sealed record DongBaoGiaRequest(string Loai, long? DichVuId, long? LinhKienId,
    string? TenHangMuc, int SoLuong, decimal DonGia);
public sealed record PhanHoiBaoGiaRequest(string QuyetDinh, string? KenhPhanHoi, string? NoiDungPhanHoi);
