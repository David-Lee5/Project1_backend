using System.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using RepairSystem.Model.ScaffoldV3.Entities;
using RepairSystem.Service.DTOs;
using RepairSystem.Service.Helpers;

namespace RepairSystem.Controllers.V3;

[ApiController]
[Route("api/v3/phieu-sua-chua/{phieuId:long}")]
[Authorize(Roles = Roles.Admin + "," + Roles.LeTan)]
public class ThanhToanController(RepairDbContext db) : ControllerBase
{
    [HttpGet("tai-chinh")]
    public async Task<IActionResult> TaiChinh(long phieuId)
    {
        if (!await db.PhieuSuaChuas.AnyAsync(x => x.Id == phieuId)) return NotFound();
        var invoice = await db.HoaDons.AsNoTracking()
            .Include(x => x.ChiTietHoaDonV3s)
            .FirstOrDefaultAsync(x => x.PhieuId == phieuId && x.TrangThai != "HUY");
        var payments = await db.GiaoDichThanhToans.AsNoTracking()
            .Where(x => x.PhieuId == phieuId).OrderBy(x => x.ThoiGian).ToListAsync();
        var daThu = payments.Sum(x => x.Loai == "THU" ? x.SoTien : -x.SoTien);
        var handover = await db.BanGiaoThietBis.AsNoTracking().FirstOrDefaultAsync(x => x.PhieuId == phieuId);
        return Ok(ApiResponse<object>.SuccessResponse(new {
            HoaDon = invoice == null ? null : new {
                invoice.Id, invoice.MaHd, invoice.TienDichVu, invoice.TienLinhKien,
                invoice.GiamGia, invoice.TongThanhToan, invoice.TrangThai,
                ChiTiet = invoice.ChiTietHoaDonV3s.Select(c => new {
                    c.Id, c.Loai, c.TenHangMuc, c.SoLuong, c.DonGia, c.ThanhTien
                })
            },
            DaThuRong = daThu,
            ConNo = invoice == null ? (decimal?)null : invoice.TongThanhToan - daThu,
            GiaoDich = payments.Select(x => new {
                x.Id, x.Loai, x.SoTien, x.PhuongThuc, x.ThoiGian, x.MaThamChieu, x.GhiChu
            }),
            BanGiao = handover == null ? null : new {
                handover.Id, handover.ThoiGian, handover.TenNguoiNhan,
                handover.TinhTrangMay, handover.PhuKienDaTra, handover.ConNoLucGiao
            }
        }));
    }

    [HttpPost("hoa-don")]
    public async Task<IActionResult> LapHoaDon(long phieuId, [FromBody] LapHoaDonRequest req)
    {
        if (req.GiamGia < 0 || req.GiamGia > 999_999_999_999.99m)
            return BadRequest(ApiResponse<object>.ErrorResponse("Giảm giá không hợp lệ."));
        var actor = await V3Auth.NhanVienIdAsync(db, User);
        if (actor == null) return Unauthorized();
        var leTanId = User.IsInRole(Roles.LeTan) ? actor.Value : req.LeTanId ?? 0;
        if (!await db.LeTans.AnyAsync(x => x.NhanVienId == leTanId))
            return BadRequest(ApiResponse<object>.ErrorResponse("Cần chọn lễ tân hợp lệ để lập hóa đơn."));
        var latestQuote = await db.BaoGia.Where(x => x.PhieuId == phieuId)
            .OrderByDescending(x => x.PhienBan).Select(x => x.TrangThai).FirstOrDefaultAsync();
        if (latestQuote != null && latestQuote != "DA_DUYET")
            return Conflict(ApiResponse<object>.ErrorResponse("Báo giá của phiếu chưa được khách chấp thuận."));
        var outId = new SqlParameter("@hoa_don_id", SqlDbType.BigInt) { Direction = ParameterDirection.Output };
        try
        {
            await db.Database.ExecuteSqlRawAsync(
                "EXEC dbo.usp_v3_lap_hoa_don @phieu_id, @le_tan_id, @giam_gia, @hoa_don_id OUTPUT",
                new SqlParameter("@phieu_id", phieuId), new SqlParameter("@le_tan_id", leTanId),
                new SqlParameter("@giam_gia", req.GiamGia), outId);
        }
        catch (SqlException ex) when (ex.Number is >= 50000 and < 52000)
        {
            return Conflict(ApiResponse<object>.ErrorResponse(ex.Message));
        }
        return Ok(ApiResponse<object>.SuccessResponse(new { HoaDonId = (long)outId.Value }));
    }

    [HttpPost("giao-dich")]
    public async Task<IActionResult> GhiGiaoDich(long phieuId, [FromBody] GiaoDichRequest req)
    {
        if (req.Loai is not ("THU" or "HOAN") || req.SoTien <= 0
            || req.SoTien > 999_999_999_999.99m
            || req.PhuongThuc is not ("TIEN_MAT" or "CHUYEN_KHOAN" or "THE" or "VI_DIEN_TU")
            || req.MaThamChieu?.Length > 100 || req.GhiChu?.Length > 500)
            return BadRequest(ApiResponse<object>.ErrorResponse("Giao dịch không hợp lệ."));
        var actor = await V3Auth.NhanVienIdAsync(db, User);
        if (actor == null) return Unauthorized();
        var outId = new SqlParameter("@giao_dich_id", SqlDbType.BigInt) { Direction = ParameterDirection.Output };
        try
        {
            await db.Database.ExecuteSqlRawAsync(
                "EXEC dbo.usp_v3_ghi_nhan_thanh_toan @phieu_id, @hoa_don_id, @loai, @so_tien, @phuong_thuc, @nguoi_thuc_hien_id, @ma_tham_chieu, @ghi_chu, @giao_dich_id OUTPUT",
                new SqlParameter("@phieu_id", phieuId),
                new SqlParameter("@hoa_don_id", (object?)req.HoaDonId ?? DBNull.Value),
                new SqlParameter("@loai", req.Loai), new SqlParameter("@so_tien", req.SoTien),
                new SqlParameter("@phuong_thuc", req.PhuongThuc),
                new SqlParameter("@nguoi_thuc_hien_id", actor.Value),
                new SqlParameter("@ma_tham_chieu", (object?)req.MaThamChieu?.Trim() ?? DBNull.Value),
                new SqlParameter("@ghi_chu", (object?)req.GhiChu?.Trim() ?? DBNull.Value), outId);
        }
        catch (SqlException ex) when (ex.Number is >= 50000 and < 52000)
        {
            return Conflict(ApiResponse<object>.ErrorResponse(ex.Message));
        }
        return Ok(ApiResponse<object>.SuccessResponse(new { GiaoDichId = (long)outId.Value }));
    }

    [HttpPost("ban-giao")]
    public async Task<IActionResult> BanGiao(long phieuId, [FromBody] BanGiaoRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.TenNguoiNhan) || req.TenNguoiNhan.Length > 150
            || req.TinhTrangMay?.Length > 1000 || req.PhuKienDaTra?.Length > 1000
            || req.XacNhan?.Length > 1000 || req.LyDoConNo?.Length > 500)
            return BadRequest(ApiResponse<object>.ErrorResponse("Thông tin bàn giao không hợp lệ."));
        var actor = await V3Auth.NhanVienIdAsync(db, User);
        if (actor == null) return Unauthorized();
        long? approver = User.IsInRole(Roles.Admin) && !string.IsNullOrWhiteSpace(req.LyDoConNo)
            ? actor : null;
        try
        {
            await using var tx = await db.Database.BeginTransactionAsync();
            await db.Database.ExecuteSqlRawAsync(
                "EXEC dbo.usp_v3_ban_giao @phieu_id, @nguoi_giao_id, @ten_nguoi_nhan, @tinh_trang_may, @phu_kien_da_tra, @xac_nhan, @nguoi_duyet_con_no_id, @ly_do_con_no",
                new SqlParameter("@phieu_id", phieuId), new SqlParameter("@nguoi_giao_id", actor.Value),
                new SqlParameter("@ten_nguoi_nhan", req.TenNguoiNhan.Trim()),
                new SqlParameter("@tinh_trang_may", (object?)req.TinhTrangMay?.Trim() ?? DBNull.Value),
                new SqlParameter("@phu_kien_da_tra", (object?)req.PhuKienDaTra?.Trim() ?? DBNull.Value),
                new SqlParameter("@xac_nhan", (object?)req.XacNhan?.Trim() ?? DBNull.Value),
                new SqlParameter("@nguoi_duyet_con_no_id", (object?)approver ?? DBNull.Value),
                new SqlParameter("@ly_do_con_no", (object?)req.LyDoConNo?.Trim() ?? DBNull.Value));
            await db.BiMatThietBiV3s.Where(x => x.PhieuId == phieuId).ExecuteDeleteAsync();
            await tx.CommitAsync();
        }
        catch (SqlException ex) when (ex.Number is >= 50000 and < 52000)
        {
            return Conflict(ApiResponse<object>.ErrorResponse(ex.Message));
        }
        return Ok(ApiResponse<object>.SuccessResponse(new { PhieuId = phieuId, TrangThai = "DA_TRA" }));
    }
}

public sealed record LapHoaDonRequest(decimal GiamGia, long? LeTanId);
public sealed record GiaoDichRequest(long? HoaDonId, string Loai, decimal SoTien,
    string PhuongThuc, string? MaThamChieu, string? GhiChu);
public sealed record BanGiaoRequest(string? TenNguoiNhan, string? TinhTrangMay,
    string? PhuKienDaTra, string? XacNhan, string? LyDoConNo);
