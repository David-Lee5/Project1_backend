using System.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RepairSystem.Model.ScaffoldV3.Entities;
using RepairSystem.Service.DTOs;
using RepairSystem.Service.Helpers;

namespace RepairSystem.Controllers.V3;

[ApiController]
[Route("api/v3/chung-tu")]
[Authorize(Roles = Roles.NhanVien)]
public sealed class ChungTuController(RepairDbContext db) : ControllerBase
{
    [HttpGet("dau-vao")]
    [Authorize(Roles = Roles.Admin + "," + Roles.ThuKho)]
    public async Task<IActionResult> DauVao([FromQuery] long? phieuNhapId)
    {
        var q = db.HoaDonDauVaos.AsNoTracking().AsQueryable();
        if (phieuNhapId.HasValue) q = q.Where(x => x.PhieuNhapId == phieuNhapId);
        return Ok(ApiResponse<object>.SuccessResponse(await q.OrderByDescending(x => x.GhiLuc)
            .Take(300).ToListAsync()));
    }

    [HttpPost("dau-vao")]
    [Authorize(Roles = Roles.Admin + "," + Roles.ThuKho)]
    public async Task<IActionResult> GhiDauVao([FromBody] GhiHoaDonDauVaoRequest req)
    {
        if (req.PhieuNhapId <= 0 || string.IsNullOrWhiteSpace(req.SoHoaDon)
            || req.SoHoaDon.Length > 100 || string.IsNullOrWhiteSpace(req.MaSoThueNcc)
            || req.MaSoThueNcc.Length > 30 || req.NgayHoaDon == default
            || req.TienHang < 0 || req.TienThue < 0 || req.TienHang > 999_999_999_999.99m
            || req.TienThue > 999_999_999_999.99m || req.GhiChu?.Length > 500)
            return BadRequest(ApiResponse<object>.ErrorResponse("Thông tin hóa đơn đầu vào không hợp lệ."));
        var actor = await NhanVienAuth.NhanVienIdAsync(db, User);
        if (actor == null) return Unauthorized();
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var receipt = await db.PhieuNhaps.AsNoTracking()
            .Include(x => x.ChiTietNhapKhos).Include(x => x.NhaCungCap)
            .FirstOrDefaultAsync(x => x.Id == req.PhieuNhapId);
        if (receipt == null || receipt.TrangThai != "DA_NHAP_KHO")
            return Conflict(ApiResponse<object>.ErrorResponse("Phiếu nhập chưa xác nhận hoặc không tồn tại."));
        if (await db.HoaDonDauVaos.AnyAsync(x => x.PhieuNhapId == req.PhieuNhapId))
            return Conflict(ApiResponse<object>.ErrorResponse("Phiếu nhập đã có hóa đơn đầu vào."));
        if (await db.HoaDonDauVaos.AsNoTracking().AnyAsync(x =>
            x.SoHoaDon == req.SoHoaDon.Trim() && db.PhieuNhaps.Any(p =>
                p.Id == x.PhieuNhapId && p.NhaCungCapId == receipt.NhaCungCapId)))
            return Conflict(ApiResponse<object>.ErrorResponse("Số hóa đơn này đã ghi cho nhà cung cấp."));
        decimal total;
        try { total = checked(req.TienHang + req.TienThue); }
        catch (OverflowException) { return BadRequest(ApiResponse<object>.ErrorResponse("Tổng tiền vượt giới hạn.")); }
        if (total > 999_999_999_999.99m)
            return BadRequest(ApiResponse<object>.ErrorResponse("Tổng tiền vượt giới hạn."));
        var stockValue = receipt.ChiTietNhapKhos.Sum(x => x.SoLuongNhap * x.DonGiaNhap);
        var delta = req.TienHang - stockValue;
        var taxMismatch = !string.IsNullOrWhiteSpace(receipt.NhaCungCap.MaSoThue)
            && !string.Equals(receipt.NhaCungCap.MaSoThue.Trim(), req.MaSoThueNcc.Trim(),
                StringComparison.OrdinalIgnoreCase);
        var status = delta == 0 && !taxMismatch ? "KHOP" : "CHENH_LECH";
        if (status == "CHENH_LECH" && string.IsNullOrWhiteSpace(req.GhiChu))
            return BadRequest(ApiResponse<object>.ErrorResponse(
                "Giá trị hoặc mã số thuế khác phiếu nhập; cần ghi lý do chênh lệch."));
        var row = new HoaDonDauVao {
            PhieuNhapId = req.PhieuNhapId, SoHoaDon = req.SoHoaDon.Trim(),
            NgayHoaDon = req.NgayHoaDon, MaSoThueNcc = req.MaSoThueNcc.Trim(),
            TienHang = req.TienHang, TienThue = req.TienThue, TongTien = total,
            TrangThai = status, ChenhLech = delta, GhiChu = req.GhiChu?.Trim(),
            NguoiGhiId = actor.Value, GhiLuc = DateTime.UtcNow
        };
        db.HoaDonDauVaos.Add(row);
        await db.SaveChangesAsync();
        await tx.CommitAsync();
        return Ok(ApiResponse<object>.SuccessResponse(new {
            row.Id, row.TrangThai, row.ChenhLech, GiaTriPhieuNhap = stockValue,
            MaSoThueKhac = taxMismatch
        }));
    }

    [HttpGet("dau-ra")]
    [Authorize(Roles = Roles.Admin + "," + Roles.LeTan)]
    public async Task<IActionResult> DauRa([FromQuery] long? hoaDonId)
    {
        var q = db.HoaDonDienTus.AsNoTracking().AsQueryable();
        if (hoaDonId.HasValue) q = q.Where(x => x.HoaDonId == hoaDonId);
        var rows = await q.OrderByDescending(x => x.GhiLuc).Take(300).Select(x => new {
            x.Id, x.HoaDonId, x.NhaCungCap, x.SoHoaDon, x.NgayPhatHanh,
            x.MaTraCuu, x.TongTien, x.TrangThai, x.NguoiGhiId, x.GhiLuc,
            DieuChinh = db.DieuChinhHoaDons.Where(a => a.HoaDonDienTuId == x.Id)
                .OrderBy(a => a.GhiLuc).Select(a => new {
                    a.Id, a.SoChungTu, a.Loai, a.LyDo, a.ChenhLechTien,
                    a.NgayChungTu, a.NguoiGhiId, a.GhiLuc
                }).ToList()
        }).ToListAsync();
        return Ok(ApiResponse<object>.SuccessResponse(rows));
    }

    [HttpPost("dau-ra")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> GhiDauRa([FromBody] GhiHoaDonDienTuRequest req)
    {
        if (req.HoaDonId <= 0 || string.IsNullOrWhiteSpace(req.NhaCungCap)
            || req.NhaCungCap.Length > 150 || string.IsNullOrWhiteSpace(req.SoHoaDon)
            || req.SoHoaDon.Length > 100 || req.MaTraCuu?.Length > 150
            || req.NgayPhatHanh == default || req.TongTien < 0)
            return BadRequest(ApiResponse<object>.ErrorResponse("Thông tin chứng từ điện tử không hợp lệ."));
        var actor = await NhanVienAuth.NhanVienIdAsync(db, User);
        if (actor == null) return Unauthorized();
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var invoice = await db.HoaDons.AsNoTracking().FirstOrDefaultAsync(x => x.Id == req.HoaDonId);
        if (invoice == null || invoice.TrangThai == "HUY")
            return Conflict(ApiResponse<object>.ErrorResponse("Hóa đơn nội bộ không hợp lệ."));
        if (invoice.TongThanhToan != req.TongTien)
            return Conflict(ApiResponse<object>.ErrorResponse("Tổng tiền khác hóa đơn nội bộ; cần đối soát trước."));
        if (await db.HoaDonDienTus.AnyAsync(x => x.HoaDonId == req.HoaDonId))
            return Conflict(ApiResponse<object>.ErrorResponse("Hóa đơn đã có chứng từ điện tử."));
        if (await db.HoaDonDienTus.AnyAsync(x =>
            x.NhaCungCap == req.NhaCungCap.Trim() && x.SoHoaDon == req.SoHoaDon.Trim()))
            return Conflict(ApiResponse<object>.ErrorResponse("Số hóa đơn đã được ghi cho đơn vị phát hành."));
        var row = new HoaDonDienTu {
            HoaDonId = req.HoaDonId, NhaCungCap = req.NhaCungCap.Trim(),
            SoHoaDon = req.SoHoaDon.Trim(), NgayPhatHanh = req.NgayPhatHanh.ToUniversalTime(),
            MaTraCuu = req.MaTraCuu?.Trim(), TongTien = req.TongTien,
            TrangThai = "DA_GHI_NHAN", NguoiGhiId = actor.Value, GhiLuc = DateTime.UtcNow
        };
        db.HoaDonDienTus.Add(row);
        await db.SaveChangesAsync();
        await tx.CommitAsync();
        return Ok(ApiResponse<object>.SuccessResponse(new { row.Id, row.HoaDonId }));
    }

    [HttpPost("dau-ra/{id:long}/dieu-chinh")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> GhiDieuChinh(long id, [FromBody] GhiDieuChinhHoaDonRequest req)
    {
        if (req.Loai is not ("DIEU_CHINH" or "THAY_THE" or "HUY")
            || string.IsNullOrWhiteSpace(req.SoChungTu) || req.SoChungTu.Length > 100
            || string.IsNullOrWhiteSpace(req.LyDo) || req.LyDo.Length > 1000
            || req.NgayChungTu == default || req.ChenhLechTien < -999_999_999_999.99m
            || req.ChenhLechTien > 999_999_999_999.99m)
            return BadRequest(ApiResponse<object>.ErrorResponse("Chứng từ điều chỉnh không hợp lệ."));
        var actor = await NhanVienAuth.NhanVienIdAsync(db, User);
        if (actor == null) return Unauthorized();
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var invoice = await db.HoaDonDienTus.FirstOrDefaultAsync(x => x.Id == id);
        if (invoice == null) return NotFound();
        if (req.Loai == "HUY" && req.ChenhLechTien != -invoice.TongTien)
            return BadRequest(ApiResponse<object>.ErrorResponse("Hủy chứng từ phải ghi giảm đúng tổng tiền đã phát hành."));
        var row = new DieuChinhHoaDon {
            HoaDonDienTuId = id, SoChungTu = req.SoChungTu.Trim(), Loai = req.Loai,
            LyDo = req.LyDo.Trim(), ChenhLechTien = req.ChenhLechTien,
            NgayChungTu = req.NgayChungTu.ToUniversalTime(),
            NguoiGhiId = actor.Value, GhiLuc = DateTime.UtcNow
        };
        db.DieuChinhHoaDons.Add(row);
        invoice.TrangThai = "DA_DIEU_CHINH";
        await db.SaveChangesAsync();
        await tx.CommitAsync();
        return Ok(ApiResponse<object>.SuccessResponse(new { row.Id, invoice.TrangThai }));
    }

    [HttpGet("doi-soat")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> DoiSoat()
    {
        var input = await db.PhieuNhaps.AsNoTracking()
            .Where(x => x.TrangThai == "DA_NHAP_KHO")
            .Select(x => new { x.Id, x.MaPn, x.NgayNhap,
                HoaDon = db.HoaDonDauVaos.Where(h => h.PhieuNhapId == x.Id)
                    .Select(h => new { h.SoHoaDon, h.TrangThai, h.ChenhLech }).FirstOrDefault() })
            .OrderByDescending(x => x.NgayNhap).Take(300).ToListAsync();
        var output = await db.HoaDons.AsNoTracking().Where(x => x.TrangThai != "HUY")
            .Select(x => new { x.Id, x.PhieuId, x.MaHd, x.TongThanhToan,
                DaThu = db.GiaoDichThanhToans.Where(p => p.PhieuId == x.PhieuId)
                    .Sum(p => (decimal?)(p.Loai == "THU" ? p.SoTien : -p.SoTien)) ?? 0m,
                ChungTu = db.HoaDonDienTus.Where(h => h.HoaDonId == x.Id)
                    .Select(h => new { h.SoHoaDon, h.TrangThai, h.TongTien,
                        DieuChinh = db.DieuChinhHoaDons.Where(a => a.HoaDonDienTuId == h.Id)
                            .Sum(a => (decimal?)a.ChenhLechTien) ?? 0m
                    }).FirstOrDefault() })
            .OrderByDescending(x => x.Id).Take(300).ToListAsync();
        var payroll = await db.CongViecSuaChuas.AsNoTracking()
            .Where(x => x.CongDuyet != null && x.ChiTraLuc == null)
            .Select(x => new { x.Id, x.PhieuId, x.KyThuatVienId, x.CongDuyet })
            .Take(300).ToListAsync();
        return Ok(ApiResponse<object>.SuccessResponse(new {
            PhieuNhap = input, HoaDonRa = output.Select(x => new {
                x.Id, x.PhieuId, x.MaHd, x.TongThanhToan, x.DaThu,
                ConNo = x.TongThanhToan - x.DaThu, x.ChungTu,
                SoTienChungTu = x.ChungTu == null ? (decimal?)null
                    : x.ChungTu.TongTien + x.ChungTu.DieuChinh,
                ChenhLechChungTu = x.ChungTu == null ? (decimal?)null
                    : x.ChungTu.TongTien + x.ChungTu.DieuChinh - x.TongThanhToan
            }), CongChoChi = payroll
        }));
    }
}

public sealed record GhiHoaDonDauVaoRequest(long PhieuNhapId, string? SoHoaDon,
    DateOnly NgayHoaDon, string? MaSoThueNcc, decimal TienHang, decimal TienThue, string? GhiChu);
public sealed record GhiHoaDonDienTuRequest(long HoaDonId, string? NhaCungCap,
    string? SoHoaDon, DateTime NgayPhatHanh, string? MaTraCuu, decimal TongTien);
public sealed record GhiDieuChinhHoaDonRequest(string? SoChungTu, string? Loai,
    string? LyDo, decimal ChenhLechTien, DateTime NgayChungTu);
