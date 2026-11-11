using System.Security.Cryptography;
using System.Text;
using System.Data;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RepairSystem.Model;
using RepairSystem.Model.ScaffoldV3.Entities;
using RepairSystem.Service.DTOs;
using RepairSystem.Service.Helpers;

namespace RepairSystem.Controllers;

[ApiController]
[Route("api/khach-hang")]
public class KhachHangController(AppDbContext db, RepairDbContext v3) : ControllerBase
{
    [HttpGet]
    [Authorize(Roles = Roles.Admin + "," + Roles.LeTan)]
    public async Task<IActionResult> TimKhach([FromQuery] string? soDienThoai)
    {
        if (string.IsNullOrWhiteSpace(soDienThoai))
            return BadRequest(ApiResponse<object>.ErrorResponse("Cần số điện thoại để tìm khách hàng."));
        var phone = Regex.Replace(soDienThoai, @"[\s.\-()]", "");
        var row = await db.KhachHangs.AsNoTracking().Where(x => x.SoDienThoai == phone && x.DeletedAt == null)
            .Select(x => new { x.Id, x.HoTen, x.SoDienThoai, DaCoTaiKhoan = x.TaiKhoanId != null })
            .FirstOrDefaultAsync();
        return row == null ? NotFound() : Ok(ApiResponse<object>.SuccessResponse(row));
    }

    [HttpPost("{id:long}/ma-lien-ket")]
    [Authorize(Roles = Roles.Admin + "," + Roles.LeTan)]
    public async Task<IActionResult> CapMaLienKet(long id)
    {
        var actorId = User.GetTaiKhoanId();
        var actor = await db.NhanViens.AsNoTracking().FirstOrDefaultAsync(x =>
            x.TaiKhoanId == actorId && x.DeletedAt == null);
        if (actor == null) return Unauthorized();
        var customer = await db.KhachHangs.AsNoTracking().FirstOrDefaultAsync(x =>
            x.Id == id && x.DeletedAt == null);
        if (customer == null) return NotFound();
        if (customer.TaiKhoanId != null)
            return Conflict(ApiResponse<object>.ErrorResponse("Khách hàng đã có tài khoản."));

        // Mã chỉ hiển thị một lần cho nhân viên sau khi xác minh khách tại quầy.
        var code = Convert.ToHexString(RandomNumberGenerator.GetBytes(16));
        await using var tx = await db.Database.BeginTransactionAsync();
        await db.MaLienKetKhachHangs.Where(x => x.KhachHangId == id && x.DaDungLuc == null)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.DaDungLuc, DateTime.UtcNow));
        var now = DateTime.UtcNow;
        db.MaLienKetKhachHangs.Add(new MaLienKetKhachHang {
            KhachHangId = id,
            MaHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(code))),
            HetHan = now.AddMinutes(15), NguoiCapId = actor.Id, CreatedAt = now
        });
        await db.SaveChangesAsync();
        await tx.CommitAsync();
        return Ok(ApiResponse<object>.SuccessResponse(new { MaLienKet = code, HetHan = now.AddMinutes(15) }));
    }

    [HttpGet("me/phieu-sua-chua")]
    [Authorize(Roles = Roles.KhachHang)]
    public async Task<IActionResult> PhieuCuaToi()
    {
        var accountId = User.GetTaiKhoanId();
        if (accountId == null) return Unauthorized();
        var rows = await v3.PhieuSuaChuas.AsNoTracking()
            .Where(x => x.ThietBi.KhachHang.TaiKhoanId == accountId)
            .OrderByDescending(x => x.NgayNhan)
            .Select(x => new { x.Id, x.MaPhieu, x.TrangThai, x.NgayNhan, x.NgayHenTra,
                x.NgayHoanThanh, x.NgayTra, x.TinhTrangLoi,
                x.ThietBiId, x.ThietBi.TenThietBi, x.ThietBi.SoSerial })
            .ToListAsync();
        return Ok(ApiResponse<object>.SuccessResponse(rows));
    }

    [HttpGet("me/phieu-sua-chua/{id:long}")]
    [Authorize(Roles = Roles.KhachHang)]
    public async Task<IActionResult> ChiTietPhieuCuaToi(long id)
    {
        var accountId = User.GetTaiKhoanId();
        if (accountId == null) return Unauthorized();
        var row = await v3.PhieuSuaChuas.AsNoTracking()
            .Where(x => x.Id == id && x.ThietBi.KhachHang.TaiKhoanId == accountId)
            .Select(x => new { x.Id, x.MaPhieu, x.TrangThai, x.NgayNhan, x.NgayHenTra,
                x.NgayHoanThanh, x.NgayTra, x.TinhTrangLoi, x.KetQuaSuaChua,
                x.TinhTrangNgoaiQuan, x.PhuKienKemTheo, x.ThietBi.TenThietBi,
                x.ThietBi.SoSerial })
            .FirstOrDefaultAsync();
        if (row == null) return NotFound();
        var history = await v3.LichSuTrangThaiPhieus.AsNoTracking()
            .Where(x => x.PhieuId == id).OrderBy(x => x.ThoiGian)
            .Select(x => new { x.TrangThaiMoi, x.ThoiGian }).ToListAsync();
        var quote = await v3.BaoGia.AsNoTracking().Where(x => x.PhieuId == id && x.TrangThai != "NHAP")
            .OrderByDescending(x => x.PhienBan)
            .Select(x => new { x.Id, x.PhienBan, x.TrangThai, x.GiamGia, x.NgayGui,
                ChiTiet = x.ChiTietBaoGia.Select(c => new { c.Loai, c.TenHangMuc, c.SoLuong, c.DonGia }) })
            .FirstOrDefaultAsync();
        return Ok(ApiResponse<object>.SuccessResponse(new { Phieu = row, LichSu = history, BaoGia = quote }));
    }

    [HttpGet("me/bao-hanh")]
    [Authorize(Roles = Roles.KhachHang)]
    public async Task<IActionResult> BaoHanhCuaToi()
    {
        var accountId = User.GetTaiKhoanId();
        if (accountId == null) return Unauthorized();
        var rows = await v3.PhieuBaoHanhs.AsNoTracking()
            .Where(x => x.PhieuSuaChua.ThietBi.KhachHang.TaiKhoanId == accountId)
            .OrderByDescending(x => x.NgayTiepNhan)
            .Select(x => new { x.Id, x.PhieuGocId, x.NgayTiepNhan, x.LoiKhachBao,
                x.TrangThai, x.KetLuan, x.KetQuaXuLy, x.NgayHoanThanh })
            .ToListAsync();
        return Ok(ApiResponse<object>.SuccessResponse(rows));
    }

    [HttpPost("me/bao-gia/{id:long}/phan-hoi")]
    [Authorize(Roles = Roles.KhachHang)]
    public async Task<IActionResult> PhanHoiBaoGia(long id, [FromBody] PhanHoiBaoGiaKhachRequest req)
    {
        if (req.QuyetDinh is not ("DA_DUYET" or "TU_CHOI") || req.NoiDung?.Length > 1000)
            return BadRequest(ApiResponse<object>.ErrorResponse("Phản hồi không hợp lệ."));
        var accountId = User.GetTaiKhoanId();
        if (accountId == null) return Unauthorized();
        var customerId = await v3.KhachHangs.AsNoTracking()
            .Where(x => x.TaiKhoanId == accountId && x.DeletedAt == null)
            .Select(x => (long?)x.Id).FirstOrDefaultAsync();
        if (customerId == null) return Forbid();
        await using var tx = await v3.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var quote = await v3.BaoGia.FirstOrDefaultAsync(x => x.Id == id
            && x.Phieu.ThietBi.KhachHangId == customerId);
        if (quote == null) return NotFound();
        if (quote.TrangThai != "CHO_DUYET"
            || quote.PhienBan != await v3.BaoGia.Where(x => x.PhieuId == quote.PhieuId).MaxAsync(x => x.PhienBan))
            return Conflict(ApiResponse<object>.ErrorResponse("Báo giá không còn chờ khách phản hồi."));
        quote.TrangThai = req.QuyetDinh;
        quote.NgayPhanHoi = DateTime.UtcNow;
        quote.KenhPhanHoi = "CONG_KHACH_HANG";
        quote.KhachHangPhanHoiId = customerId;
        quote.NoiDungPhanHoi = req.NoiDung?.Trim();
        await v3.SaveChangesAsync();
        await tx.CommitAsync();
        return Ok(ApiResponse<object>.SuccessResponse(new { quote.Id, quote.TrangThai }));
    }
}

public sealed record PhanHoiBaoGiaKhachRequest(string QuyetDinh, string? NoiDung);
