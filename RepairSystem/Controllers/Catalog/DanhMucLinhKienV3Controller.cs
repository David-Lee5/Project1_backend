using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RepairSystem.Model.ScaffoldV3.Entities;
using RepairSystem.Service.DTOs;
using RepairSystem.Service.Helpers;

namespace RepairSystem.Controllers.V3;

[ApiController]
[Route("api/v3/loai-linh-kien")]
[Authorize(Roles = Roles.NhanVien)]
public class LoaiLinhKienV3Controller(AppDbContextV3 db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> DanhSach()
    {
        var rows = await db.LoaiLinhKiens.AsNoTracking()
            .Include(x => x.ThuocTinhLoaiLinhKiens)
            .ThenInclude(x => x.LuaChonThuocTinhLinhKiens)
            .OrderBy(x => x.Ten).ToListAsync();
        return Ok(ApiResponse<object>.SuccessResponse(rows.Select(x => new {
            x.Id, x.Ma, x.Ten, x.HoatDong,
            ThuocTinh = x.ThuocTinhLoaiLinhKiens.OrderBy(t => t.ThuTu).Select(t => new {
                t.Id, t.Ma, t.Ten, t.KieuDuLieu, t.DonVi, t.BatBuoc, t.ThuTu, t.HoatDong,
                LuaChon = t.LuaChonThuocTinhLinhKiens.OrderBy(c => c.ThuTu)
                    .Select(c => new { c.Id, c.GiaTri, c.ThuTu, c.HoatDong })
            })
        }).ToList()));
    }

    [HttpPost]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> TaoLoai([FromBody] TaoLoaiRequest req)
    {
        var ma = req.Ma?.Trim().ToUpperInvariant();
        var ten = req.Ten?.Trim();
        if (string.IsNullOrWhiteSpace(ma) || ma.Length > 40 || string.IsNullOrWhiteSpace(ten) || ten.Length > 100)
            return BadRequest(ApiResponse<object>.ErrorResponse("Mã và tên loại không hợp lệ."));
        if (await db.LoaiLinhKiens.AnyAsync(x => x.Ma == ma))
            return Conflict(ApiResponse<object>.ErrorResponse("Mã loại đã tồn tại."));
        var row = new LoaiLinhKien { Ma = ma, Ten = ten, HoatDong = true };
        db.LoaiLinhKiens.Add(row);
        await db.SaveChangesAsync();
        return Ok(ApiResponse<object>.SuccessResponse(new { row.Id, row.Ma, row.Ten }));
    }

    [HttpPut("{id:long}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> CapNhatLoai(long id, [FromBody] CapNhatLoaiRequest req)
    {
        var ma = req.Ma?.Trim().ToUpperInvariant();
        var ten = req.Ten?.Trim();
        if (string.IsNullOrWhiteSpace(ma) || ma.Length > 40 || string.IsNullOrWhiteSpace(ten) || ten.Length > 100)
            return BadRequest(ApiResponse<object>.ErrorResponse("Mã và tên loại không hợp lệ."));
        var row = await db.LoaiLinhKiens.FirstOrDefaultAsync(x => x.Id == id);
        if (row == null) return NotFound();
        if (await db.LoaiLinhKiens.AnyAsync(x => x.Id != id && x.Ma == ma))
            return Conflict(ApiResponse<object>.ErrorResponse("Mã loại đã tồn tại."));
        row.Ma = ma;
        row.Ten = ten;
        row.HoatDong = req.HoatDong;
        await db.SaveChangesAsync();
        return Ok(ApiResponse<object>.SuccessResponse(new { row.Id, row.Ma, row.Ten, row.HoatDong }));
    }

    [HttpPost("{loaiId:long}/thuoc-tinh")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> TaoThuocTinh(long loaiId, [FromBody] TaoThuocTinhRequest req)
    {
        if (!await db.LoaiLinhKiens.AnyAsync(x => x.Id == loaiId && x.HoatDong)) return NotFound();
        var ma = req.Ma?.Trim().ToUpperInvariant();
        var ten = req.Ten?.Trim();
        var kieu = req.KieuDuLieu?.Trim().ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(ma) || ma.Length > 40 || string.IsNullOrWhiteSpace(ten) || ten.Length > 100
            || kieu is not ("VAN_BAN" or "SO" or "LUA_CHON" or "DUNG_SAI")
            || req.DonVi?.Length > 30)
            return BadRequest(ApiResponse<object>.ErrorResponse("Thông tin thuộc tính không hợp lệ."));
        if (await db.ThuocTinhLoaiLinhKiens.AnyAsync(x => x.LoaiLinhKienId == loaiId && x.Ma == ma))
            return Conflict(ApiResponse<object>.ErrorResponse("Mã thuộc tính đã tồn tại trong loại này."));
        var row = new ThuocTinhLoaiLinhKien {
            LoaiLinhKienId = loaiId, Ma = ma, Ten = ten, KieuDuLieu = kieu,
            DonVi = req.DonVi?.Trim(), BatBuoc = req.BatBuoc, ThuTu = req.ThuTu, HoatDong = true
        };
        db.ThuocTinhLoaiLinhKiens.Add(row);
        await db.SaveChangesAsync();
        return Ok(ApiResponse<object>.SuccessResponse(new { row.Id, row.Ma, row.Ten }));
    }

    [HttpPut("thuoc-tinh/{id:long}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> CapNhatThuocTinh(long id, [FromBody] CapNhatThuocTinhRequest req)
    {
        var ma = req.Ma?.Trim().ToUpperInvariant();
        var ten = req.Ten?.Trim();
        var kieu = req.KieuDuLieu?.Trim().ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(ma) || ma.Length > 40 || string.IsNullOrWhiteSpace(ten) || ten.Length > 100
            || kieu is not ("VAN_BAN" or "SO" or "LUA_CHON" or "DUNG_SAI") || req.DonVi?.Length > 30)
            return BadRequest(ApiResponse<object>.ErrorResponse("Thông tin thuộc tính không hợp lệ."));
        var row = await db.ThuocTinhLoaiLinhKiens.FirstOrDefaultAsync(x => x.Id == id);
        if (row == null) return NotFound();
        if (await db.ThuocTinhLoaiLinhKiens.AnyAsync(x => x.Id != id && x.LoaiLinhKienId == row.LoaiLinhKienId && x.Ma == ma))
            return Conflict(ApiResponse<object>.ErrorResponse("Mã thuộc tính đã tồn tại trong loại này."));
        if (row.KieuDuLieu != kieu && (await db.ThongSoLinhKiens.AnyAsync(x => x.ThuocTinhId == id)
            || await db.LuaChonThuocTinhLinhKiens.AnyAsync(x => x.ThuocTinhId == id)))
            return Conflict(ApiResponse<object>.ErrorResponse("Thuộc tính đang được dùng; không thể đổi kiểu dữ liệu."));
        if (req.BatBuoc && req.HoatDong && (!row.BatBuoc || !row.HoatDong)
            && await db.LinhKiens.AnyAsync(x => x.LoaiLinhKienId == row.LoaiLinhKienId && x.DeletedAt == null
                && !x.ThongSoLinhKiens.Any(s => s.ThuocTinhId == id)))
            return Conflict(ApiResponse<object>.ErrorResponse("Có mẫu linh kiện chưa có giá trị cho thuộc tính bắt buộc này."));
        row.Ma = ma;
        row.Ten = ten;
        row.KieuDuLieu = kieu;
        row.DonVi = req.DonVi?.Trim();
        row.BatBuoc = req.BatBuoc;
        row.ThuTu = req.ThuTu;
        row.HoatDong = req.HoatDong;
        await db.SaveChangesAsync();
        return Ok(ApiResponse<object>.SuccessResponse(new { row.Id, row.Ma, row.Ten, row.HoatDong }));
    }

    [HttpPost("thuoc-tinh/{thuocTinhId:long}/lua-chon")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> TaoLuaChon(long thuocTinhId, [FromBody] TaoLuaChonRequest req)
    {
        var attr = await db.ThuocTinhLoaiLinhKiens.FirstOrDefaultAsync(x => x.Id == thuocTinhId);
        if (attr == null) return NotFound();
        if (attr.KieuDuLieu != "LUA_CHON")
            return BadRequest(ApiResponse<object>.ErrorResponse("Thuộc tính này không dùng danh sách lựa chọn."));
        var giaTri = req.GiaTri?.Trim();
        if (string.IsNullOrWhiteSpace(giaTri) || giaTri.Length > 150)
            return BadRequest(ApiResponse<object>.ErrorResponse("Giá trị lựa chọn không hợp lệ."));
        if (await db.LuaChonThuocTinhLinhKiens.AnyAsync(x => x.ThuocTinhId == thuocTinhId && x.GiaTri == giaTri))
            return Conflict(ApiResponse<object>.ErrorResponse("Lựa chọn đã tồn tại."));
        var row = new LuaChonThuocTinhLinhKien {
            ThuocTinhId = thuocTinhId, GiaTri = giaTri, ThuTu = req.ThuTu, HoatDong = true
        };
        db.LuaChonThuocTinhLinhKiens.Add(row);
        await db.SaveChangesAsync();
        return Ok(ApiResponse<object>.SuccessResponse(new { row.Id, row.GiaTri }));
    }

    [HttpPut("lua-chon/{id:long}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> CapNhatLuaChon(long id, [FromBody] CapNhatLuaChonRequest req)
    {
        var giaTri = req.GiaTri?.Trim();
        if (string.IsNullOrWhiteSpace(giaTri) || giaTri.Length > 150)
            return BadRequest(ApiResponse<object>.ErrorResponse("Giá trị lựa chọn không hợp lệ."));
        var row = await db.LuaChonThuocTinhLinhKiens.FirstOrDefaultAsync(x => x.Id == id);
        if (row == null) return NotFound();
        if (await db.LuaChonThuocTinhLinhKiens.AnyAsync(x => x.Id != id && x.ThuocTinhId == row.ThuocTinhId && x.GiaTri == giaTri))
            return Conflict(ApiResponse<object>.ErrorResponse("Lựa chọn đã tồn tại."));
        row.GiaTri = giaTri;
        row.ThuTu = req.ThuTu;
        row.HoatDong = req.HoatDong;
        await db.SaveChangesAsync();
        return Ok(ApiResponse<object>.SuccessResponse(new { row.Id, row.GiaTri, row.HoatDong }));
    }
}

public sealed record TaoLoaiRequest(string? Ma, string? Ten);
public sealed record CapNhatLoaiRequest(string? Ma, string? Ten, bool HoatDong);
public sealed record TaoThuocTinhRequest(string? Ma, string? Ten, string? KieuDuLieu,
    string? DonVi, bool BatBuoc, int ThuTu);
public sealed record CapNhatThuocTinhRequest(string? Ma, string? Ten, string? KieuDuLieu,
    string? DonVi, bool BatBuoc, int ThuTu, bool HoatDong);
public sealed record TaoLuaChonRequest(string? GiaTri, int ThuTu);
public sealed record CapNhatLuaChonRequest(string? GiaTri, int ThuTu, bool HoatDong);

[ApiController]
[Route("api/v3/mau-linh-kien")]
[Authorize(Roles = Roles.NhanVien)]
public class MauLinhKienV3Controller(AppDbContextV3 db) : ControllerBase
{
    [HttpGet("{id:long}")]
    public async Task<IActionResult> ChiTiet(long id)
    {
        var row = await db.LinhKiens.AsNoTracking()
            .Include(x => x.LoaiLinhKien)
            .Include(x => x.ThongSoLinhKiens).ThenInclude(x => x.ThuocTinh)
            .Include(x => x.ThongSoLinhKiens).ThenInclude(x => x.LuaChonThuocTinhLinhKien)
            .FirstOrDefaultAsync(x => x.Id == id && x.DeletedAt == null);
        if (row == null) return NotFound();
        return Ok(ApiResponse<object>.SuccessResponse(new {
            row.Id, row.MaLk, row.TenLinhKien, row.HangSanXuat, row.MaModel,
            row.DonViTinh, row.DonGiaBan, row.ThoiHanBaoHanhThang, row.TheoDoiSerial,
            Loai = new { row.LoaiLinhKienId, Ten = row.LoaiLinhKien?.Ten },
            ThongSo = row.ThongSoLinhKiens.Select(s => new {
                s.ThuocTinhId, s.ThuocTinh.Ten, s.GiaTriVanBan, s.GiaTriSo,
                s.GiaTriDungSai, s.LuaChonId,
                GiaTriLuaChon = s.LuaChonThuocTinhLinhKien?.GiaTri
            })
        }));
    }

    [HttpPost]
    [Authorize(Roles = Roles.Admin + "," + Roles.ThuKho)]
    public async Task<IActionResult> TaoMau([FromBody] TaoMauLinhKienRequest req)
    {
        if (req.LoaiLinhKienId <= 0 || string.IsNullOrWhiteSpace(req.TenLinhKien)
            || req.TenLinhKien.Length > 150 || string.IsNullOrWhiteSpace(req.DonViTinh)
            || req.DonViTinh.Length > 20 || req.HangSanXuat?.Length > 100 || req.MaModel?.Length > 100
            || req.DonGiaBan < 0 || req.DonGiaBan > 999_999_999_999.99m
            || req.ThoiHanBaoHanhThang is < 0 or > 120)
            return BadRequest(ApiResponse<object>.ErrorResponse("Thông tin mẫu linh kiện không hợp lệ."));

        var loai = await db.LoaiLinhKiens.AsNoTracking()
            .Include(x => x.ThuocTinhLoaiLinhKiens)
            .ThenInclude(x => x.LuaChonThuocTinhLinhKiens)
            .FirstOrDefaultAsync(x => x.Id == req.LoaiLinhKienId && x.HoatDong);
        if (loai == null) return BadRequest(ApiResponse<object>.ErrorResponse("Loại linh kiện không tồn tại hoặc đã ngừng dùng."));

        var values = req.ThongSo ?? new();
        if (values.GroupBy(x => x.ThuocTinhId).Any(g => g.Count() != 1))
            return BadRequest(ApiResponse<object>.ErrorResponse("Một thuộc tính không được nhập nhiều lần."));
        foreach (var spec in values)
        {
            var attr = loai.ThuocTinhLoaiLinhKiens.FirstOrDefault(x => x.Id == spec.ThuocTinhId && x.HoatDong);
            if (attr == null) return BadRequest(ApiResponse<object>.ErrorResponse("Thông số không thuộc loại linh kiện đã chọn."));
            var count = (spec.GiaTriVanBan != null ? 1 : 0) + (spec.GiaTriSo.HasValue ? 1 : 0)
                + (spec.GiaTriDungSai.HasValue ? 1 : 0) + (spec.LuaChonId.HasValue ? 1 : 0);
            if (count != 1 || (attr.KieuDuLieu == "VAN_BAN" && string.IsNullOrWhiteSpace(spec.GiaTriVanBan))
                || (attr.KieuDuLieu == "SO" && !spec.GiaTriSo.HasValue)
                || (attr.KieuDuLieu == "DUNG_SAI" && !spec.GiaTriDungSai.HasValue)
                || (attr.KieuDuLieu == "LUA_CHON" && (!spec.LuaChonId.HasValue
                    || !attr.LuaChonThuocTinhLinhKiens.Any(c => c.Id == spec.LuaChonId && c.HoatDong)))
                || (attr.KieuDuLieu != "VAN_BAN" && spec.GiaTriVanBan != null)
                || (attr.KieuDuLieu != "SO" && spec.GiaTriSo.HasValue)
                || (attr.KieuDuLieu != "DUNG_SAI" && spec.GiaTriDungSai.HasValue)
                || (attr.KieuDuLieu != "LUA_CHON" && spec.LuaChonId.HasValue))
                return BadRequest(ApiResponse<object>.ErrorResponse($"Giá trị thông số {attr.Ten} không đúng kiểu."));
        }
        if (loai.ThuocTinhLoaiLinhKiens.Any(x => x.HoatDong && x.BatBuoc
            && values.All(v => v.ThuocTinhId != x.Id)))
            return BadRequest(ApiResponse<object>.ErrorResponse("Chưa nhập đủ thông số bắt buộc."));

        await using var tx = await db.Database.BeginTransactionAsync();
        var row = new LinhKien {
            LoaiLinhKienId = loai.Id, TenLinhKien = req.TenLinhKien.Trim(),
            HangSanXuat = req.HangSanXuat?.Trim(), MaModel = req.MaModel?.Trim(),
            DonViTinh = req.DonViTinh.Trim(), DonGiaBan = req.DonGiaBan,
            ThoiHanBaoHanhThang = (short)req.ThoiHanBaoHanhThang,
            TheoDoiSerial = req.TheoDoiSerial
        };
        db.LinhKiens.Add(row);
        await db.SaveChangesAsync();
        foreach (var spec in values)
            db.ThongSoLinhKiens.Add(new ThongSoLinhKien {
                LinhKienId = row.Id, ThuocTinhId = spec.ThuocTinhId,
                GiaTriVanBan = spec.GiaTriVanBan?.Trim(), GiaTriSo = spec.GiaTriSo,
                GiaTriDungSai = spec.GiaTriDungSai, LuaChonId = spec.LuaChonId
            });
        await db.SaveChangesAsync();
        await db.Database.ExecuteSqlInterpolatedAsync($"EXEC dbo.usp_v3_kiem_tra_linh_kien @linh_kien_id={row.Id}");
        await tx.CommitAsync();
        return CreatedAtAction(nameof(ChiTiet), new { id = row.Id },
            ApiResponse<object>.SuccessResponse(new { row.Id, row.MaLk }, "Đã tạo mẫu linh kiện."));
    }

    [HttpPut("{id:long}")]
    [Authorize(Roles = Roles.Admin + "," + Roles.ThuKho)]
    public async Task<IActionResult> CapNhatMau(long id, [FromBody] TaoMauLinhKienRequest req)
    {
        var row = await db.LinhKiens.Include(x => x.ThongSoLinhKiens)
            .FirstOrDefaultAsync(x => x.Id == id && x.DeletedAt == null);
        if (row == null) return NotFound();
        if (req.LoaiLinhKienId <= 0 || string.IsNullOrWhiteSpace(req.TenLinhKien)
            || req.TenLinhKien.Length > 150 || string.IsNullOrWhiteSpace(req.DonViTinh)
            || req.DonViTinh.Length > 20 || req.HangSanXuat?.Length > 100 || req.MaModel?.Length > 100
            || req.DonGiaBan < 0 || req.DonGiaBan > 999_999_999_999.99m
            || req.ThoiHanBaoHanhThang is < 0 or > 120)
            return BadRequest(ApiResponse<object>.ErrorResponse("Thông tin mẫu linh kiện không hợp lệ."));
        if (row.LoaiLinhKienId != req.LoaiLinhKienId && row.ThongSoLinhKiens.Count != 0)
            return Conflict(ApiResponse<object>.ErrorResponse("Mẫu đã có thông số; không thể đổi loại linh kiện."));
        if (row.TheoDoiSerial != req.TheoDoiSerial && (await db.ChiTietNhapKhos.AnyAsync(x => x.LinhKienId == id)
            || await db.ChiTietXuatKhos.AnyAsync(x => x.LinhKienId == id)
            || await db.LinhKienSerials.AnyAsync(x => x.LinhKienId == id)
            || await db.TonKhos.AnyAsync(x => x.LinhKienId == id && x.SoLuongTon != 0)))
            return Conflict(ApiResponse<object>.ErrorResponse("Linh kiện đã có lịch sử kho; không thể đổi chính sách serial."));

        var loai = await db.LoaiLinhKiens.AsNoTracking()
            .Include(x => x.ThuocTinhLoaiLinhKiens)
            .ThenInclude(x => x.LuaChonThuocTinhLinhKiens)
            .FirstOrDefaultAsync(x => x.Id == req.LoaiLinhKienId && (x.HoatDong || row.LoaiLinhKienId == x.Id));
        if (loai == null) return BadRequest(ApiResponse<object>.ErrorResponse("Loại linh kiện không tồn tại hoặc đã ngừng dùng."));
        var values = req.ThongSo ?? new();
        if (values.GroupBy(x => x.ThuocTinhId).Any(g => g.Count() != 1))
            return BadRequest(ApiResponse<object>.ErrorResponse("Một thuộc tính không được nhập nhiều lần."));
        foreach (var spec in values)
        {
            var attr = loai.ThuocTinhLoaiLinhKiens.FirstOrDefault(x => x.Id == spec.ThuocTinhId && x.HoatDong);
            if (attr == null) return BadRequest(ApiResponse<object>.ErrorResponse("Thông số không thuộc loại linh kiện đang dùng."));
            var count = (spec.GiaTriVanBan != null ? 1 : 0) + (spec.GiaTriSo.HasValue ? 1 : 0)
                + (spec.GiaTriDungSai.HasValue ? 1 : 0) + (spec.LuaChonId.HasValue ? 1 : 0);
            if (count != 1 || (attr.KieuDuLieu == "VAN_BAN" && string.IsNullOrWhiteSpace(spec.GiaTriVanBan))
                || (attr.KieuDuLieu == "SO" && !spec.GiaTriSo.HasValue)
                || (attr.KieuDuLieu == "DUNG_SAI" && !spec.GiaTriDungSai.HasValue)
                || (attr.KieuDuLieu == "LUA_CHON" && (!spec.LuaChonId.HasValue
                    || !attr.LuaChonThuocTinhLinhKiens.Any(c => c.Id == spec.LuaChonId && c.HoatDong)))
                || (attr.KieuDuLieu != "VAN_BAN" && spec.GiaTriVanBan != null)
                || (attr.KieuDuLieu != "SO" && spec.GiaTriSo.HasValue)
                || (attr.KieuDuLieu != "DUNG_SAI" && spec.GiaTriDungSai.HasValue)
                || (attr.KieuDuLieu != "LUA_CHON" && spec.LuaChonId.HasValue))
                return BadRequest(ApiResponse<object>.ErrorResponse($"Giá trị thông số {attr.Ten} không đúng kiểu."));
        }
        if (loai.ThuocTinhLoaiLinhKiens.Any(x => x.HoatDong && x.BatBuoc
            && values.All(v => v.ThuocTinhId != x.Id)))
            return BadRequest(ApiResponse<object>.ErrorResponse("Chưa nhập đủ thông số bắt buộc."));

        await using var tx = await db.Database.BeginTransactionAsync();
        row.LoaiLinhKienId = req.LoaiLinhKienId;
        row.TenLinhKien = req.TenLinhKien.Trim();
        row.HangSanXuat = req.HangSanXuat?.Trim();
        row.MaModel = req.MaModel?.Trim();
        row.DonViTinh = req.DonViTinh.Trim();
        row.DonGiaBan = req.DonGiaBan;
        row.ThoiHanBaoHanhThang = (short)req.ThoiHanBaoHanhThang;
        row.TheoDoiSerial = req.TheoDoiSerial;
        row.UpdatedAt = DateTime.UtcNow;
        var activeIds = loai.ThuocTinhLoaiLinhKiens.Where(x => x.HoatDong).Select(x => x.Id).ToHashSet();
        db.ThongSoLinhKiens.RemoveRange(row.ThongSoLinhKiens.Where(x => activeIds.Contains(x.ThuocTinhId)));
        await db.SaveChangesAsync();
        foreach (var spec in values)
            db.ThongSoLinhKiens.Add(new ThongSoLinhKien {
                LinhKienId = id, ThuocTinhId = spec.ThuocTinhId,
                GiaTriVanBan = spec.GiaTriVanBan?.Trim(), GiaTriSo = spec.GiaTriSo,
                GiaTriDungSai = spec.GiaTriDungSai, LuaChonId = spec.LuaChonId
            });
        await db.SaveChangesAsync();
        await db.Database.ExecuteSqlInterpolatedAsync($"EXEC dbo.usp_v3_kiem_tra_linh_kien @linh_kien_id={id}");
        await tx.CommitAsync();
        return Ok(ApiResponse<object>.SuccessResponse(new { row.Id, row.MaLk }, "Đã cập nhật mẫu linh kiện."));
    }
}

public sealed record TaoMauLinhKienRequest(long LoaiLinhKienId, string? TenLinhKien,
    string? HangSanXuat, string? MaModel, string? DonViTinh, decimal DonGiaBan,
    int ThoiHanBaoHanhThang, bool TheoDoiSerial, List<GiaTriThongSoRequest>? ThongSo);
public sealed record GiaTriThongSoRequest(long ThuocTinhId, string? GiaTriVanBan,
    decimal? GiaTriSo, bool? GiaTriDungSai, long? LuaChonId);
