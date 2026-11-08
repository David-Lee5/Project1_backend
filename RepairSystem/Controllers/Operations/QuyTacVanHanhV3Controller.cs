using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RepairSystem.Model.ScaffoldV3.Entities;
using RepairSystem.Service.DTOs;
using RepairSystem.Service.Helpers;

namespace RepairSystem.Controllers.V3;

[ApiController]
[Route("api/v3/quy-tac-van-hanh")]
[Authorize(Roles = Roles.NhanVien)]
public sealed class QuyTacVanHanhV3Controller(AppDbContextV3 db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var row = await db.QuyTacVanHanhV3s.AsNoTracking().SingleAsync(x => x.Id == 1);
        return Ok(ApiResponse<object>.SuccessResponse(row));
    }

    [HttpPut]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Put([FromBody] SuaQuyTacVanHanhRequest req)
    {
        if (req.BaoHanhLapDuyetTuLan is < 2 or > 10 || req.AnhTiepNhanToiThieu is < 1 or > 5
            || string.IsNullOrWhiteSpace(req.QuyTacTraCong) || req.QuyTacTraCong.Length > 500
            || string.IsNullOrWhiteSpace(req.ChinhSachSerial) || req.ChinhSachSerial.Length > 500
            || req.LoaiHinhKinhDoanh?.Length > 100 || req.PhuongPhapThue?.Length > 100
            || req.DonViHoaDonDienTu?.Length > 150)
            return BadRequest(ApiResponse<object>.ErrorResponse("Quy tắc không hợp lệ."));
        var actor = await V3Auth.NhanVienIdAsync(db, User);
        if (actor == null) return Unauthorized();
        var row = await db.QuyTacVanHanhV3s.SingleAsync(x => x.Id == 1);
        var before = JsonSerializer.Serialize(row);
        row.BaoHanhLapDuyetTuLan = req.BaoHanhLapDuyetTuLan;
        row.AnhTiepNhanToiThieu = req.AnhTiepNhanToiThieu;
        row.QuyTacTraCong = req.QuyTacTraCong.Trim();
        row.ChinhSachSerial = req.ChinhSachSerial.Trim();
        row.LoaiHinhKinhDoanh = NullIfBlank(req.LoaiHinhKinhDoanh);
        row.PhuongPhapThue = NullIfBlank(req.PhuongPhapThue);
        row.DonViHoaDonDienTu = NullIfBlank(req.DonViHoaDonDienTu);
        row.NguoiCapNhatId = actor;
        row.CapNhatLuc = DateTime.UtcNow;
        db.AuditLogs.Add(new AuditLog {
            Bang = "quy_tac_van_hanh_v3", BanGhiId = 1, HanhDong = "UPDATE",
            DuLieuCu = before, DuLieuMoi = JsonSerializer.Serialize(row),
            NguoiThucHienId = User.GetTaiKhoanId()
        });
        await db.SaveChangesAsync();
        return Ok(ApiResponse<object>.SuccessResponse(row));
    }

    private static string? NullIfBlank(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

public sealed record SuaQuyTacVanHanhRequest(byte BaoHanhLapDuyetTuLan,
    byte AnhTiepNhanToiThieu, string QuyTacTraCong, string ChinhSachSerial,
    string? LoaiHinhKinhDoanh, string? PhuongPhapThue, string? DonViHoaDonDienTu);
