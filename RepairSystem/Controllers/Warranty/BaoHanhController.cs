using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RepairSystem.Model.ScaffoldV3.Entities;
using RepairSystem.Service.DTOs;
using RepairSystem.Service.Helpers;

namespace RepairSystem.Controllers.V3;

[ApiController]
[Route("api/v3/bao-hanh")]
[Authorize(Roles = Roles.NhanVien)]
public class BaoHanhController(RepairDbContext db) : ControllerBase
{
    private static readonly Dictionary<string, string[]> BuocKhach = new() {
        ["TIEP_NHAN"] = ["DANG_KIEM_TRA"],
        ["DANG_KIEM_TRA"] = ["CHAP_NHAN", "TU_CHOI"],
        ["CHAP_NHAN"] = ["DANG_XU_LY"],
        ["DANG_XU_LY"] = ["HOAN_THANH"],
        ["HOAN_THANH"] = ["DA_TRA"]
    };

    [HttpGet("cam-ket/{phieuGocId:long}")]
    public async Task<IActionResult> CamKet(long phieuGocId)
    {
        var rows = await db.CamKetBaoHanhs.AsNoTracking()
            .Where(x => x.PhieuGocId == phieuGocId).OrderBy(x => x.NgayKetThuc)
            .Select(x => new { x.Id, x.Loai, x.ChiTietXuatKhoId, x.DichVuId,
                x.NgayBatDau, x.NgayKetThuc, x.DieuKien }).ToListAsync();
        return Ok(ApiResponse<object>.SuccessResponse(rows));
    }

    [HttpPost("cam-ket")]
    [Authorize(Roles = Roles.Admin + "," + Roles.LeTan)]
    public async Task<IActionResult> TaoCamKet([FromBody] TaoCamKetBaoHanhRequest req)
    {
        if (req.NgayKetThuc < req.NgayBatDau || req.DieuKien?.Length > 1000
            || !((req.Loai == "DICH_VU" && req.DichVuId > 0 && req.ChiTietXuatKhoId == null)
                 || (req.Loai == "LINH_KIEN" && req.ChiTietXuatKhoId > 0 && req.DichVuId == null)))
            return BadRequest(ApiResponse<object>.ErrorResponse("Cam kết bảo hành không hợp lệ."));
        var phieu = await db.PhieuSuaChuas.AsNoTracking().FirstOrDefaultAsync(x => x.Id == req.PhieuGocId);
        if (phieu == null || phieu.TrangThai != "DA_TRA")
            return Conflict(ApiResponse<object>.ErrorResponse("Chỉ tạo cam kết cho phiếu đã bàn giao."));
        var row = new CamKetBaoHanh {
            PhieuGocId = req.PhieuGocId, Loai = req.Loai,
            DichVuId = req.DichVuId, ChiTietXuatKhoId = req.ChiTietXuatKhoId,
            NgayBatDau = req.NgayBatDau, NgayKetThuc = req.NgayKetThuc,
            DieuKien = req.DieuKien?.Trim()
        };
        db.CamKetBaoHanhs.Add(row);
        try { await db.SaveChangesAsync(); }
        catch (DbUpdateException) { return Conflict(ApiResponse<object>.ErrorResponse("Hạng mục bảo hành không thuộc phiếu hoặc đã có cam kết.")); }
        return Ok(ApiResponse<object>.SuccessResponse(new { row.Id }));
    }

    [HttpGet("khach")]
    public async Task<IActionResult> DanhSachKhach([FromQuery] long? phieuGocId,
        [FromQuery] long? phieuId)
    {
        var q = db.PhieuBaoHanhs.AsNoTracking().AsQueryable();
        if (phieuGocId.HasValue) q = q.Where(x => x.PhieuGocId == phieuGocId);
        if (phieuId.HasValue) q = q.Where(x => x.PhieuGocId == phieuId
            || x.PhieuSuaBaoHanhId == phieuId);
        var rows = await q.OrderByDescending(x => x.NgayTiepNhan)
            .Select(x => new { x.Id, x.PhieuGocId, x.ThietBiId, x.CamKetId,
                x.PhieuSuaBaoHanhId, x.NgayTiepNhan, x.LoiKhachBao,
                x.TrangThai, x.KetLuan, x.KetQuaXuLy, x.NgayHoanThanh,
                PheDuyetLap = db.DuyetBaoHanhLaps.Where(a => a.PhieuBaoHanhId == x.Id)
                    .Select(a => new { a.LanThu, a.TrangThai, a.NguyenNhanSoBo,
                        a.LyDoQuyetDinh, a.NguoiDuyetId, a.NgayDuyet }).FirstOrDefault(),
                PhanTich = db.BaoHanhPhanTichs.Where(a => a.PhieuBaoHanhId == x.Id)
                    .Select(a => new { a.NguyenNhan, a.BenChiuChiPhi,
                        a.ChiPhiDuKien, a.ChiPhiThucTe, a.NguoiGhiId, a.CapNhatLuc }).FirstOrDefault()
            }).ToListAsync();
        return Ok(ApiResponse<object>.SuccessResponse(rows));
    }

    [HttpPost("khach")]
    [Authorize(Roles = Roles.Admin + "," + Roles.LeTan)]
    public async Task<IActionResult> TiepNhanKhach([FromBody] TiepNhanBaoHanhRequest req)
    {
        if (req.PhieuGocId <= 0 || string.IsNullOrWhiteSpace(req.LoiKhachBao)
            || req.LoiKhachBao.Length > 1000)
            return BadRequest(ApiResponse<object>.ErrorResponse("Thông tin tiếp nhận bảo hành không hợp lệ."));
        var actor = await NhanVienAuth.NhanVienIdAsync(db, User);
        if (actor == null) return Unauthorized();
        await using var tx = await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
        var phieu = await db.PhieuSuaChuas.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == req.PhieuGocId);
        if (phieu == null || phieu.TrangThai != "DA_TRA")
            return Conflict(ApiResponse<object>.ErrorResponse("Phiếu gốc chưa bàn giao hoặc không tồn tại."));
        if (req.CamKetId.HasValue)
        {
            var camKet = await db.CamKetBaoHanhs.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == req.CamKetId && x.PhieuGocId == req.PhieuGocId);
            if (camKet == null) return BadRequest(ApiResponse<object>.ErrorResponse("Cam kết không thuộc phiếu gốc."));
            var today = DateOnly.FromDateTime(DateTime.UtcNow.AddHours(7));
            if (today < camKet.NgayBatDau || today > camKet.NgayKetThuc)
                return BadRequest(ApiResponse<object>.ErrorResponse("Cam kết chưa có hiệu lực hoặc đã hết hạn."));
        }
        if (req.PhieuSuaBaoHanhId.HasValue
            && !await db.PhieuSuaChuas.AnyAsync(x => x.Id == req.PhieuSuaBaoHanhId
                && x.ThietBiId == phieu.ThietBiId && x.Id != phieu.Id))
            return BadRequest(ApiResponse<object>.ErrorResponse("Phiếu sửa bảo hành không cùng thiết bị."));
        if (req.PhieuSuaBaoHanhId.HasValue && await db.PhieuBaoHanhs
            .AnyAsync(x => x.PhieuSuaBaoHanhId == req.PhieuSuaBaoHanhId))
            return Conflict(ApiResponse<object>.ErrorResponse("Phiếu sửa đã liên kết với hồ sơ bảo hành khác."));
        var row = new PhieuBaoHanh {
            PhieuGocId = phieu.Id, ThietBiId = phieu.ThietBiId,
            CamKetId = req.CamKetId, PhieuSuaBaoHanhId = req.PhieuSuaBaoHanhId,
            NguoiTiepNhanId = actor.Value, LoiKhachBao = req.LoiKhachBao.Trim(),
            TrangThai = "TIEP_NHAN"
        };
        db.PhieuBaoHanhs.Add(row);
        await db.SaveChangesAsync();
        var lanThu = checked(await db.PhieuBaoHanhs.CountAsync(x => x.PhieuGocId == phieu.Id));
        var threshold = await db.QuyTacVanHanhs.AsNoTracking()
            .Where(x => x.Id == 1).Select(x => x.BaoHanhLapDuyetTuLan).SingleAsync();
        var approval = new DuyetBaoHanhLap {
            PhieuBaoHanhId = row.Id, LanThu = lanThu,
            TrangThai = lanThu >= threshold ? "CHO_DUYET" : "KHONG_CAN_DUYET",
            NguoiDeXuatId = actor.Value
        };
        db.DuyetBaoHanhLaps.Add(approval);
        await db.SaveChangesAsync();
        await tx.CommitAsync();
        return Ok(ApiResponse<object>.SuccessResponse(new {
            row.Id, row.TrangThai, LanThu = lanThu, TrangThaiPheDuyet = approval.TrangThai }));
    }

    [HttpPost("khach/{id:long}/duyet-lap-lai")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> DuyetBaoHanhLap(long id, [FromBody] DuyetBaoHanhLapRequest req)
    {
        if (req.QuyetDinh is not ("DA_DUYET" or "TU_CHOI")
            || string.IsNullOrWhiteSpace(req.NguyenNhanSoBo) || req.NguyenNhanSoBo.Length > 1000
            || string.IsNullOrWhiteSpace(req.LyDoQuyetDinh) || req.LyDoQuyetDinh.Length > 1000)
            return BadRequest(ApiResponse<object>.ErrorResponse("Cần kết luận nguyên nhân và lý do quyết định."));
        var actor = await NhanVienAuth.NhanVienIdAsync(db, User);
        if (actor == null) return Unauthorized();
        await using var tx = await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
        var approval = await db.DuyetBaoHanhLaps.FirstOrDefaultAsync(x => x.PhieuBaoHanhId == id);
        if (approval == null) return NotFound();
        if (approval.TrangThai != "CHO_DUYET")
            return Conflict(ApiResponse<object>.ErrorResponse("Hồ sơ không chờ duyệt bảo hành lặp lại."));
        if (approval.NguoiDeXuatId == actor)
            return Conflict(ApiResponse<object>.ErrorResponse("Người tiếp nhận không được tự duyệt hồ sơ của mình."));
        approval.TrangThai = req.QuyetDinh;
        approval.NguyenNhanSoBo = req.NguyenNhanSoBo.Trim();
        approval.LyDoQuyetDinh = req.LyDoQuyetDinh.Trim();
        approval.NguoiDuyetId = actor;
        approval.NgayDuyet = DateTime.UtcNow;
        await db.SaveChangesAsync();
        await tx.CommitAsync();
        return Ok(ApiResponse<object>.SuccessResponse(new { approval.PhieuBaoHanhId, approval.TrangThai }));
    }

    [HttpPatch("khach/{id:long}/lien-ket-phieu-sua")]
    [Authorize(Roles = Roles.Admin + "," + Roles.LeTan)]
    public async Task<IActionResult> LienKetPhieuSua(long id, [FromBody] LienKetPhieuSuaBaoHanhRequest req)
    {
        if (req.PhieuSuaBaoHanhId <= 0)
            return BadRequest(ApiResponse<object>.ErrorResponse("Cần phiếu sửa bảo hành hợp lệ."));
        await using var tx = await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
        var claim = await db.PhieuBaoHanhs.FirstOrDefaultAsync(x => x.Id == id);
        if (claim == null) return NotFound();
        if (claim.PhieuSuaBaoHanhId.HasValue || claim.TrangThai is "HOAN_THANH" or "DA_TRA" or "TU_CHOI")
            return Conflict(ApiResponse<object>.ErrorResponse("Hồ sơ đã liên kết hoặc đã kết thúc."));
        if (!await db.PhieuSuaChuas.AsNoTracking().AnyAsync(x =>
            x.Id == req.PhieuSuaBaoHanhId && x.Id != claim.PhieuGocId
            && x.ThietBiId == claim.ThietBiId && x.TrangThai != "HUY"))
            return BadRequest(ApiResponse<object>.ErrorResponse("Phiếu sửa không cùng thiết bị hoặc đã hủy."));
        if (await db.PhieuBaoHanhs.AnyAsync(x => x.PhieuSuaBaoHanhId == req.PhieuSuaBaoHanhId))
            return Conflict(ApiResponse<object>.ErrorResponse("Phiếu sửa đã thuộc hồ sơ bảo hành khác."));
        claim.PhieuSuaBaoHanhId = req.PhieuSuaBaoHanhId;
        await db.SaveChangesAsync();
        await tx.CommitAsync();
        return Ok(ApiResponse<object>.SuccessResponse(new { claim.Id, claim.PhieuSuaBaoHanhId }));
    }

    [HttpPost("khach/{id:long}/trang-thai")]
    [Authorize(Roles = Roles.Admin + "," + Roles.LeTan + "," + Roles.KyThuatVien)]
    public async Task<IActionResult> DoiTrangThaiKhach(long id, [FromBody] DoiTrangThaiBaoHanhRequest req)
    {
        await using var tx = await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
        var row = await db.PhieuBaoHanhs.FirstOrDefaultAsync(x => x.Id == id);
        if (row == null) return NotFound();
        var approval = await db.DuyetBaoHanhLaps.AsNoTracking()
            .FirstOrDefaultAsync(x => x.PhieuBaoHanhId == id);
        if (approval?.TrangThai is "CHO_DUYET" or "TU_CHOI")
            return Conflict(ApiResponse<object>.ErrorResponse("Bảo hành lặp lại chưa được quản lý chấp thuận."));
        if (!BuocKhach.TryGetValue(row.TrangThai, out var next) || !next.Contains(req.TrangThai))
            return Conflict(ApiResponse<object>.ErrorResponse("Bước chuyển trạng thái không hợp lệ."));
        if (req.KetLuan?.Length > 1000 || req.KetQuaXuLy?.Length > 1000)
            return BadRequest(ApiResponse<object>.ErrorResponse("Nội dung quá dài."));
        if (req.TrangThai == "TU_CHOI" && string.IsNullOrWhiteSpace(req.KetLuan))
            return BadRequest(ApiResponse<object>.ErrorResponse("Cần kết luận khi từ chối bảo hành."));
        if (req.TrangThai == "HOAN_THANH" && string.IsNullOrWhiteSpace(req.KetQuaXuLy))
            return BadRequest(ApiResponse<object>.ErrorResponse("Cần ghi kết quả xử lý."));
        if (req.TrangThai == "HOAN_THANH")
        {
            var analysis = await db.BaoHanhPhanTichs.AsNoTracking()
                .FirstOrDefaultAsync(x => x.PhieuBaoHanhId == id);
            if (analysis?.ChiPhiThucTe == null)
                return Conflict(ApiResponse<object>.ErrorResponse(
                    "Cần ghi nguyên nhân, bên chịu chi phí và chi phí thực tế trước khi hoàn thành bảo hành."));
            if (row.PhieuSuaBaoHanhId.HasValue)
            {
                var repairStatus = await db.PhieuSuaChuas.AsNoTracking()
                    .Where(x => x.Id == row.PhieuSuaBaoHanhId)
                    .Select(x => x.TrangThai).SingleAsync();
                if (repairStatus is not ("HOAN_THANH" or "DA_TRA")
                    || !await db.CongViecSuaChuas.AsNoTracking()
                        .AnyAsync(x => x.PhieuBaoHanhId == id && x.TrangThai == "HOAN_THANH")
                    || await db.CongViecSuaChuas.AsNoTracking()
                        .AnyAsync(x => x.PhieuBaoHanhId == id
                            && (x.TrangThai == "CHO_LAM" || x.TrangThai == "DANG_LAM")))
                    return Conflict(ApiResponse<object>.ErrorResponse(
                        "Phiếu sửa bảo hành cần hoàn thành ít nhất một công việc liên kết."));
            }
        }
        row.TrangThai = req.TrangThai;
        if (req.KetLuan != null) row.KetLuan = req.KetLuan.Trim();
        if (req.KetQuaXuLy != null) row.KetQuaXuLy = req.KetQuaXuLy.Trim();
        if (req.TrangThai == "HOAN_THANH") row.NgayHoanThanh = DateTime.UtcNow;
        await db.SaveChangesAsync();
        await tx.CommitAsync();
        return Ok(ApiResponse<object>.SuccessResponse(new { row.Id, row.TrangThai }));
    }

    [HttpPut("khach/{id:long}/phan-tich-chi-phi")]
    [Authorize(Roles = Roles.Admin + "," + Roles.LeTan)]
    public async Task<IActionResult> PhanTichChiPhi(long id, [FromBody] PhanTichBaoHanhRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.NguyenNhan) || req.NguyenNhan.Length > 1000
            || req.BenChiuChiPhi is not ("CUA_HANG" or "KHACH" or "NCC" or "KTV")
            || req.ChiPhiDuKien < 0 || req.ChiPhiDuKien > 999_999_999_999.99m
            || req.ChiPhiThucTe < 0 || req.ChiPhiThucTe > 999_999_999_999.99m)
            return BadRequest(ApiResponse<object>.ErrorResponse("Phân tích chi phí bảo hành không hợp lệ."));
        var actor = await NhanVienAuth.NhanVienIdAsync(db, User);
        if (actor == null) return Unauthorized();
        await using var tx = await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
        var claim = await db.PhieuBaoHanhs.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (claim == null) return NotFound();
        if (claim.TrangThai is "DA_TRA" or "TU_CHOI")
            return Conflict(ApiResponse<object>.ErrorResponse("Hồ sơ bảo hành đã kết thúc."));
        var row = await db.BaoHanhPhanTichs.FirstOrDefaultAsync(x => x.PhieuBaoHanhId == id);
        if (row == null)
        {
            row = new BaoHanhPhanTich { PhieuBaoHanhId = id };
            db.BaoHanhPhanTichs.Add(row);
        }
        row.NguyenNhan = req.NguyenNhan.Trim();
        row.BenChiuChiPhi = req.BenChiuChiPhi;
        row.ChiPhiDuKien = req.ChiPhiDuKien;
        row.ChiPhiThucTe = req.ChiPhiThucTe;
        row.NguoiGhiId = actor.Value;
        row.CapNhatLuc = DateTime.UtcNow;
        await db.SaveChangesAsync();
        await tx.CommitAsync();
        return Ok(ApiResponse<object>.SuccessResponse(row));
    }

    [HttpGet("nha-cung-cap")]
    [Authorize(Roles = Roles.Admin + "," + Roles.ThuKho)]
    public async Task<IActionResult> DanhSachNcc([FromQuery] long? phieuBaoHanhId)
    {
        var q = db.PhieuBaoHanhNccs.AsNoTracking().AsQueryable();
        if (phieuBaoHanhId.HasValue) q = q.Where(x => x.PhieuBaoHanhId == phieuBaoHanhId);
        var rows = await q.OrderByDescending(x => x.Id).Select(x => new {
            x.Id, x.PhieuBaoHanhId, x.NhaCungCapId, x.LinhKienId, x.LinhKienSerialId,
            x.SoLuong, x.TrangThai, x.NgayGui, x.NgayNhanLai, x.KetQua,
            x.KhoGuiId, x.DaTruTon, x.NguonHang,
            XuLyKho = db.XuLyBaoHanhNccs.Where(y => y.PhieuBaoHanhNccId == x.Id)
                .Select(y => new { y.Id, y.Loai, y.KhoId, y.PhieuNhapId,
                    y.SoLuong, y.ThoiGian, y.GhiChu }).FirstOrDefault()
        }).ToListAsync();
        return Ok(ApiResponse<object>.SuccessResponse(rows));
    }

    [HttpPost("nha-cung-cap")]
    [Authorize(Roles = Roles.Admin + "," + Roles.ThuKho)]
    public async Task<IActionResult> TaoNcc([FromBody] TaoBaoHanhNccRequest req)
    {
        if (req.SoLuong <= 0 || (req.LinhKienSerialId.HasValue && req.SoLuong != 1))
            return BadRequest(ApiResponse<object>.ErrorResponse("Số lượng bảo hành nhà cung cấp không hợp lệ."));
        if (!await db.NhaCungCaps.AnyAsync(x => x.Id == req.NhaCungCapId && x.DeletedAt == null)
            || !await db.LinhKiens.AnyAsync(x => x.Id == req.LinhKienId && x.DeletedAt == null))
            return BadRequest(ApiResponse<object>.ErrorResponse("Nhà cung cấp hoặc linh kiện không tồn tại."));
        if (req.PhieuBaoHanhId.HasValue && !await db.PhieuBaoHanhs.AnyAsync(x => x.Id == req.PhieuBaoHanhId))
            return BadRequest(ApiResponse<object>.ErrorResponse("Phiếu bảo hành khách không tồn tại."));
        if (req.LinhKienSerialId.HasValue && !await db.LinhKienSerials.AnyAsync(x =>
            x.Id == req.LinhKienSerialId && x.LinhKienId == req.LinhKienId))
            return BadRequest(ApiResponse<object>.ErrorResponse("Serial không thuộc linh kiện."));
        var row = new PhieuBaoHanhNcc {
            PhieuBaoHanhId = req.PhieuBaoHanhId, NhaCungCapId = req.NhaCungCapId,
            LinhKienId = req.LinhKienId, LinhKienSerialId = req.LinhKienSerialId,
            SoLuong = req.SoLuong, TrangThai = "CHO_GUI", NguonHang = "CU"
        };
        db.PhieuBaoHanhNccs.Add(row);
        await db.SaveChangesAsync();
        return Ok(ApiResponse<object>.SuccessResponse(new { row.Id, row.TrangThai }));
    }

    [HttpPost("nha-cung-cap/{id:long}/gui")]
    [Authorize(Roles = Roles.Admin + "," + Roles.ThuKho)]
    public async Task<IActionResult> GuiNcc(long id,
        [FromBody(EmptyBodyBehavior = Microsoft.AspNetCore.Mvc.ModelBinding.EmptyBodyBehavior.Allow)] GuiBaoHanhNccRequest? req)
    {
        var actor = await NhanVienAuth.NhanVienIdAsync(db, User);
        if (actor == null) return Unauthorized();
        await using var tx = await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
        var row = await db.PhieuBaoHanhNccs.FirstOrDefaultAsync(x => x.Id == id);
        if (row == null) return NotFound();
        if (row.TrangThai != "CHO_GUI") return Conflict(ApiResponse<object>.ErrorResponse("Hồ sơ không ở trạng thái chờ gửi."));
        var tracked = await db.LinhKiens.Where(x => x.Id == row.LinhKienId)
            .Select(x => x.TheoDoiSerial).FirstAsync();
        if (tracked == true && row.LinhKienSerialId == null)
            return BadRequest(ApiResponse<object>.ErrorResponse("Mẫu linh kiện theo dõi serial; cần chọn serial trước khi gửi."));
        var fromCustomer = req?.Nguon == "KHACH";
        if (req?.Nguon is not null and not ("KHO" or "KHACH"))
            return BadRequest(ApiResponse<object>.ErrorResponse("Nguồn linh kiện không hợp lệ."));
        if (fromCustomer && row.PhieuBaoHanhId == null)
            return BadRequest(ApiResponse<object>.ErrorResponse("Hàng của khách phải gắn phiếu bảo hành khách."));
        long? khoId = null;
        if (row.LinhKienSerialId.HasValue)
        {
            var serial = await db.LinhKienSerials.FirstAsync(x => x.Id == row.LinhKienSerialId);
            if (serial.TrangThai == "TRONG_KHO" && !fromCustomer)
                khoId = serial.KhoId;
            else if (serial.TrangThai != "DA_XUAT" || !fromCustomer)
                return Conflict(ApiResponse<object>.ErrorResponse("Trạng thái serial không khớp nguồn hàng."));
            if (fromCustomer && !await db.PhieuBaoHanhs.AnyAsync(x => x.Id == row.PhieuBaoHanhId
                && db.ChiTietXuatKhos.Any(c => c.PhieuId == x.PhieuGocId
                    && c.LinhKienSerialId == row.LinhKienSerialId)))
                return Conflict(ApiResponse<object>.ErrorResponse("Serial không thuộc phiếu bảo hành khách."));
            serial.TrangThai = "BAO_HANH";
            serial.KhoId = null;
            serial.UpdatedAt = DateTime.UtcNow;
        }
        else if (!fromCustomer)
            khoId = req?.KhoId;
        else if (!await db.PhieuBaoHanhs.AnyAsync(x => x.Id == row.PhieuBaoHanhId
            && db.ChiTietXuatKhos.Any(c => c.PhieuId == x.PhieuGocId && c.LinhKienId == row.LinhKienId)))
            return Conflict(ApiResponse<object>.ErrorResponse("Linh kiện không thuộc phiếu bảo hành khách."));

        if (!fromCustomer)
        {
            if (khoId == null || !await db.Khos.AnyAsync(x => x.Id == khoId && x.DeletedAt == null))
                return BadRequest(ApiResponse<object>.ErrorResponse("Cần chọn kho gửi hàng hợp lệ."));
            var stock = await db.TonKhos.FirstOrDefaultAsync(x => x.KhoId == khoId && x.LinhKienId == row.LinhKienId);
            if (stock == null || stock.SoLuongTon < row.SoLuong)
                return Conflict(ApiResponse<object>.ErrorResponse("Tồn kho không đủ để gửi bảo hành."));
            stock.SoLuongTon -= row.SoLuong;
            stock.NgayCapNhat = DateTime.UtcNow;
            db.BienDongKhos.Add(new BienDongKho {
                KhoId = khoId.Value, LinhKienId = row.LinhKienId, Loai = "DIEU_CHINH",
                SoLuongThayDoi = -row.SoLuong, SoLuongSau = stock.SoLuongTon,
                NguoiThucHienId = actor.Value, LyDo = $"Gửi bảo hành NCC #{row.Id}",
                ThoiGian = DateTime.UtcNow
            });
            row.DaTruTon = true;
            row.KhoGuiId = khoId;
        }
        row.TrangThai = "DA_GUI";
        row.NguonHang = fromCustomer ? "KHACH" : "KHO";
        row.NgayGui = DateTime.UtcNow;
        await db.SaveChangesAsync();
        await tx.CommitAsync();
        return Ok(ApiResponse<object>.SuccessResponse(new { row.Id, row.TrangThai }));
    }

    [HttpPost("nha-cung-cap/{id:long}/nhan-lai")]
    [Authorize(Roles = Roles.Admin + "," + Roles.ThuKho)]
    public async Task<IActionResult> NhanLaiNcc(long id, [FromBody] NhanLaiBaoHanhNccRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.KetQua) || req.KetQua.Length > 1000)
            return BadRequest(ApiResponse<object>.ErrorResponse("Cần ghi kết quả nhà cung cấp."));
        var row = await db.PhieuBaoHanhNccs.FirstOrDefaultAsync(x => x.Id == id);
        if (row == null) return NotFound();
        if (row.TrangThai != "DA_GUI") return Conflict(ApiResponse<object>.ErrorResponse("Hồ sơ chưa được gửi."));
        row.TrangThai = "DA_NHAN_LAI";
        row.NgayNhanLai = DateTime.UtcNow;
        row.KetQua = req.KetQua.Trim();
        await db.SaveChangesAsync();
        return Ok(ApiResponse<object>.SuccessResponse(new { row.Id, row.TrangThai }));
    }

    [HttpPost("nha-cung-cap/{id:long}/xu-ly-kho")]
    [Authorize(Roles = Roles.Admin + "," + Roles.ThuKho)]
    public async Task<IActionResult> XuLyKho(long id, [FromBody] XuLyKhoBaoHanhNccRequest req)
    {
        if (req.Loai is not ("NHAP_LAI" or "NHAP_THAY_THE" or "KHONG_NHAP")
            || req.GhiChu?.Length > 500)
            return BadRequest(ApiResponse<object>.ErrorResponse("Kết quả xử lý kho không hợp lệ."));
        var actor = await NhanVienAuth.NhanVienIdAsync(db, User);
        if (actor == null) return Unauthorized();
        await using var tx = await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
        var row = await db.PhieuBaoHanhNccs.FirstOrDefaultAsync(x => x.Id == id);
        if (row == null) return NotFound();
        if (row.TrangThai != "DA_NHAN_LAI"
            || await db.XuLyBaoHanhNccs.AnyAsync(x => x.PhieuBaoHanhNccId == id))
            return Conflict(ApiResponse<object>.ErrorResponse("Hồ sơ chưa nhận lại hoặc đã xử lý kho."));
        if (row.NguonHang == "CU")
            return Conflict(ApiResponse<object>.ErrorResponse(
                "Hồ sơ cũ chưa xác định nguồn hàng; cần đối soát thủ công trước khi xử lý tồn."));

        if (req.Loai == "NHAP_LAI")
        {
            if (req.KhoId == null || req.PhieuNhapId != null
                || !await db.Khos.AnyAsync(x => x.Id == req.KhoId && x.DeletedAt == null))
                return BadRequest(ApiResponse<object>.ErrorResponse("Cần kho nhập lại hợp lệ."));
            var stock = await db.TonKhos.FirstOrDefaultAsync(x => x.KhoId == req.KhoId && x.LinhKienId == row.LinhKienId);
            if (stock == null)
            {
                stock = new TonKho { KhoId = req.KhoId.Value, LinhKienId = row.LinhKienId,
                    SoLuongTon = 0, MucTonToiThieu = 0, NgayCapNhat = DateTime.UtcNow };
                db.TonKhos.Add(stock);
            }
            stock.SoLuongTon = checked(stock.SoLuongTon + row.SoLuong);
            stock.NgayCapNhat = DateTime.UtcNow;
            if (row.LinhKienSerialId.HasValue)
            {
                var serial = await db.LinhKienSerials.FirstAsync(x => x.Id == row.LinhKienSerialId);
                if (serial.TrangThai != "BAO_HANH")
                    return Conflict(ApiResponse<object>.ErrorResponse("Serial không còn trong luồng bảo hành."));
                serial.TrangThai = "TRONG_KHO";
                serial.KhoId = req.KhoId;
                serial.ChiTietXuatKhoId = null;
                serial.UpdatedAt = DateTime.UtcNow;
            }
            db.BienDongKhos.Add(new BienDongKho {
                KhoId = req.KhoId.Value, LinhKienId = row.LinhKienId, Loai = "DIEU_CHINH",
                SoLuongThayDoi = row.SoLuong, SoLuongSau = stock.SoLuongTon,
                NguoiThucHienId = actor.Value, LyDo = $"Nhập lại bảo hành NCC #{id}",
                ThoiGian = DateTime.UtcNow
            });
        }
        else if (req.Loai == "NHAP_THAY_THE")
        {
            if (req.PhieuNhapId == null || req.KhoId != null
                || !await db.PhieuNhaps.AnyAsync(x => x.Id == req.PhieuNhapId
                    && x.TrangThai == "DA_NHAP_KHO" && x.NhaCungCapId == row.NhaCungCapId
                    && x.ChiTietNhapKhos.Any(c => c.LinhKienId == row.LinhKienId
                        && c.SoLuongNhap >= row.SoLuong)))
                return BadRequest(ApiResponse<object>.ErrorResponse("Cần phiếu nhập thay thế đã xác nhận, cùng NCC và linh kiện."));
        }
        else if (req.KhoId != null || req.PhieuNhapId != null || string.IsNullOrWhiteSpace(req.GhiChu))
            return BadRequest(ApiResponse<object>.ErrorResponse("Cần lý do khi không nhập lại."));

        if (req.Loai != "NHAP_LAI" && row.LinhKienSerialId.HasValue)
        {
            var serial = await db.LinhKienSerials.FirstAsync(x => x.Id == row.LinhKienSerialId);
            if (serial.TrangThai != "BAO_HANH")
                return Conflict(ApiResponse<object>.ErrorResponse("Serial không còn trong luồng bảo hành."));
            serial.TrangThai = "THANH_LY";
            serial.UpdatedAt = DateTime.UtcNow;
        }
        var disposition = new XuLyBaoHanhNcc {
            PhieuBaoHanhNccId = id, Loai = req.Loai, KhoId = req.KhoId,
            PhieuNhapId = req.PhieuNhapId, SoLuong = row.SoLuong,
            NguoiThucHienId = actor.Value, ThoiGian = DateTime.UtcNow,
            GhiChu = req.GhiChu?.Trim()
        };
        db.XuLyBaoHanhNccs.Add(disposition);
        await db.SaveChangesAsync();
        await tx.CommitAsync();
        return Ok(ApiResponse<object>.SuccessResponse(new { disposition.Id, disposition.Loai }));
    }
}

public sealed record TaoCamKetBaoHanhRequest(long PhieuGocId, string Loai,
    long? ChiTietXuatKhoId, long? DichVuId, DateOnly NgayBatDau, DateOnly NgayKetThuc, string? DieuKien);
public sealed record TiepNhanBaoHanhRequest(long PhieuGocId, long? CamKetId,
    long? PhieuSuaBaoHanhId, string? LoiKhachBao);
public sealed record LienKetPhieuSuaBaoHanhRequest(long PhieuSuaBaoHanhId);
public sealed record DoiTrangThaiBaoHanhRequest(string TrangThai, string? KetLuan, string? KetQuaXuLy);
public sealed record PhanTichBaoHanhRequest(string? NguyenNhan, string? BenChiuChiPhi,
    decimal ChiPhiDuKien, decimal? ChiPhiThucTe);
public sealed record DuyetBaoHanhLapRequest(string QuyetDinh, string? NguyenNhanSoBo,
    string? LyDoQuyetDinh);
public sealed record TaoBaoHanhNccRequest(long? PhieuBaoHanhId, long NhaCungCapId,
    long LinhKienId, long? LinhKienSerialId, int SoLuong);
public sealed record NhanLaiBaoHanhNccRequest(string? KetQua);
public sealed record GuiBaoHanhNccRequest(string? Nguon, long? KhoId);
public sealed record XuLyKhoBaoHanhNccRequest(string Loai, long? KhoId, long? PhieuNhapId, string? GhiChu);
