using Microsoft.EntityFrameworkCore;
using RepairSystem.Model;
using RepairSystem.Service.DTOs;
using RepairSystem.Service.IServices;
using RepairSystem.Service.Helpers;

namespace RepairSystem.Service.Services;

public class LinhKienService : ILinhKienService
{
    private readonly AppDbContext _context;

    public LinhKienService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<LinhKienDto>> GetAllAsync(string? keyword = null, bool includeStock = false, bool includeDeleted = false)
    {
        var query = _context.LinhKiens.AsNoTracking().AsQueryable();

        if (!includeDeleted)
            query = query.Where(l => l.DeletedAt == null);

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var k = keyword.Trim();
            query = query.Where(l =>
                l.TenLinhKien.Contains(k) ||
                l.MaLk.Contains(k) ||
                (l.HangSanXuat != null && l.HangSanXuat.Contains(k)));
        }

        return await Project(query.OrderBy(l => l.TenLinhKien), includeStock).ToListAsync();
    }

    public async Task<LinhKienDto?> GetByIdAsync(long id, bool includeStock = false)
    {
        var dto = await Project(_context.LinhKiens.AsNoTracking()
                .Where(l => l.Id == id && l.DeletedAt == null), includeStock)
            .FirstOrDefaultAsync();

        return dto;
    }

    public async Task<LinhKienDto> CreateAsync(UpsertLinhKienDto dto)
    {
        CatalogValidation.Validate(dto);
        CatalogValidation.Money(dto.DonGiaBan);
        var lk = new LinhKien
        {
            TenLinhKien = dto.TenLinhKien.Trim(),
            HangSanXuat = NullIfBlank(dto.HangSanXuat),
            MoTa = NullIfBlank(dto.MoTa),
            DonViTinh = dto.DonViTinh.Trim(),
            DonGiaBan = dto.DonGiaBan,
            ThoiHanBaoHanhThang = (short)dto.ThoiHanBaoHanhThang,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.LinhKiens.Add(lk);
        await _context.SaveChangesAsync();

        // Linh kiện mới chưa nhập kho nên tồn = 0. Dòng ton_kho sẽ tự được tạo khi xác nhận phiếu nhập đầu tiên.
        return ToDto(lk, tongTon: 0);
    }

    public async Task<LinhKienDto> UpdateAsync(long id, UpsertLinhKienDto dto)
    {
        CatalogValidation.Validate(dto);
        CatalogValidation.Money(dto.DonGiaBan);
        var lk = await _context.LinhKiens
            .FirstOrDefaultAsync(l => l.Id == id && l.DeletedAt == null)
            ?? throw new KeyNotFoundException($"Không tìm thấy linh kiện id={id}.");

        lk.TenLinhKien = dto.TenLinhKien.Trim();
        lk.HangSanXuat = NullIfBlank(dto.HangSanXuat);
        lk.MoTa = NullIfBlank(dto.MoTa);
        lk.DonViTinh = dto.DonViTinh.Trim();
        lk.DonGiaBan = dto.DonGiaBan;
        lk.ThoiHanBaoHanhThang = (short)dto.ThoiHanBaoHanhThang;
        lk.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return await Project(_context.LinhKiens.AsNoTracking().Where(l => l.Id == id), includeStock: true).SingleAsync();
    }

    public async Task<bool> DeleteAsync(long id)
    {
        await using var tx = _context.Database.IsRelational()
            ? await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable)
            : null;
        var lk = await _context.LinhKiens
            .FirstOrDefaultAsync(l => l.Id == id && l.DeletedAt == null);

        if (lk == null) return false;

        if (await _context.TonKhos.AnyAsync(t => t.LinhKienId == id && t.SoLuongTon > 0))
            throw new InvalidOperationException("Linh kiện vẫn còn tồn kho nên chưa thể xoá.");
        if (await _context.ChiTietNhapKhos.AnyAsync(c => c.LinhKienId == id && c.PhieuNhap.TrangThai == "NHAP"))
            throw new InvalidOperationException("Linh kiện còn trong phiếu nhập nháp. Hãy xử lý phiếu nhập trước khi xoá.");

        lk.DeletedAt = DateTime.UtcNow;
        lk.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        if (tx != null) await tx.CommitAsync();
        return true;
    }

    public async Task<IReadOnlyList<AnhLinhKienDto>> GetImagesAsync(long partId)
    {
        if (!await _context.LinhKiens.AnyAsync(l => l.Id == partId && l.DeletedAt == null))
            throw new KeyNotFoundException("Không tìm thấy linh kiện.");
        return await _context.AnhLinhKiens.AsNoTracking().Where(a => a.LinhKienId == partId)
            .OrderByDescending(a => a.LaAnhChinh).ThenBy(a => a.ThuTu).ThenBy(a => a.Id)
            .Select(a => new AnhLinhKienDto
            {
                Id = a.Id, LinhKienId = a.LinhKienId, DuongDan = a.DuongDan,
                LaAnhChinh = a.LaAnhChinh, ThuTu = a.ThuTu, CreatedAt = a.CreatedAt
            }).ToListAsync();
    }

    public async Task<AnhLinhKienDto> AddImageAsync(long partId, CreateAnhLinhKienDto dto)
    {
        CatalogValidation.Validate(dto);
        var url = dto.DuongDan.Trim();
        if (!Uri.TryCreate(url, UriKind.Absolute, out var parsed) ||
            (parsed.Scheme != Uri.UriSchemeHttp && parsed.Scheme != Uri.UriSchemeHttps) ||
            !string.IsNullOrEmpty(parsed.UserInfo))
            throw new ArgumentException("Ảnh phải có URL HTTP hoặc HTTPS hợp lệ và không chứa thông tin đăng nhập.");

        await using var tx = _context.Database.IsRelational()
            ? await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable) : null;
        var part = await LockPartAsync(partId);
        var existing = await _context.AnhLinhKiens.Where(a => a.LinhKienId == partId).ToListAsync();
        if (existing.Any(a => a.DuongDan == url))
            throw new InvalidOperationException("URL ảnh đã có trong danh sách của linh kiện.");

        var primary = dto.LaAnhChinh || existing.Count == 0;
        if (primary)
        {
            existing.ForEach(a => a.LaAnhChinh = false);
            // Flush the old primary before inserting the new one to preserve the filtered unique index.
            await _context.SaveChangesAsync();
        }
        var image = new AnhLinhKien
        {
            LinhKienId = partId, DuongDan = url, LaAnhChinh = primary,
            ThuTu = (short)dto.ThuTu, CreatedAt = DateTime.UtcNow
        };
        _context.AnhLinhKiens.Add(image);
        part.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        if (tx != null) await tx.CommitAsync();
        return ImageDto(image);
    }

    public async Task<AnhLinhKienDto> SetPrimaryImageAsync(long partId, long imageId)
    {
        await using var tx = _context.Database.IsRelational()
            ? await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable) : null;
        var part = await LockPartAsync(partId);
        var images = await _context.AnhLinhKiens.Where(a => a.LinhKienId == partId).ToListAsync();
        var image = images.SingleOrDefault(a => a.Id == imageId)
            ?? throw new KeyNotFoundException("Không tìm thấy ảnh của linh kiện.");
        images.ForEach(a => a.LaAnhChinh = false);
        await _context.SaveChangesAsync();
        image.LaAnhChinh = true;
        part.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        if (tx != null) await tx.CommitAsync();
        return ImageDto(image);
    }

    public async Task<bool> DeleteImageAsync(long partId, long imageId)
    {
        await using var tx = _context.Database.IsRelational()
            ? await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable) : null;
        var part = await LockPartAsync(partId);
        var images = await _context.AnhLinhKiens.Where(a => a.LinhKienId == partId)
            .OrderBy(a => a.ThuTu).ThenBy(a => a.Id).ToListAsync();
        var image = images.SingleOrDefault(a => a.Id == imageId);
        if (image == null) return false;
        _context.AnhLinhKiens.Remove(image);
        await _context.SaveChangesAsync();
        if (image.LaAnhChinh && images.FirstOrDefault(a => a.Id != imageId) is { } next)
            next.LaAnhChinh = true;
        part.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        if (tx != null) await tx.CommitAsync();
        return true;
    }

    private async Task<LinhKien> LockPartAsync(long id)
    {
        var query = _context.Database.IsSqlServer()
            ? _context.LinhKiens.FromSqlInterpolated($"SELECT * FROM dbo.linh_kien WITH (UPDLOCK, HOLDLOCK) WHERE id = {id}")
            : _context.LinhKiens.Where(l => l.Id == id);
        var part = await query.SingleOrDefaultAsync();
        if (part == null || part.DeletedAt != null) throw new KeyNotFoundException("Không tìm thấy linh kiện.");
        return part;
    }

    private static AnhLinhKienDto ImageDto(AnhLinhKien image) => new()
    {
        Id = image.Id, LinhKienId = image.LinhKienId, DuongDan = image.DuongDan,
        LaAnhChinh = image.LaAnhChinh, ThuTu = image.ThuTu, CreatedAt = image.CreatedAt
    };

    // ── helpers ────────────────────────────────────────────────────
    private static IQueryable<LinhKienDto> Project(IQueryable<LinhKien> query, bool includeStock) =>
        query.Select(l => new LinhKienDto
        {
            Id = l.Id,
            MaLk = l.MaLk,
            TenLinhKien = l.TenLinhKien,
            HangSanXuat = l.HangSanXuat,
            MoTa = l.MoTa,
            HinhAnh = l.AnhLinhKiens.Where(a => a.LaAnhChinh).Select(a => a.DuongDan).FirstOrDefault(),
            DonViTinh = l.DonViTinh,
            DonGiaBan = l.DonGiaBan,
            ThoiHanBaoHanhThang = l.ThoiHanBaoHanhThang,
            TongTon = includeStock ? l.TonKhos.Sum(t => (long)t.SoLuongTon) : null,
            CreatedAt = l.CreatedAt,
            UpdatedAt = l.UpdatedAt
        });

    private static string? NullIfBlank(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static LinhKienDto ToDto(LinhKien l, long tongTon) => new()
    {
        Id = l.Id,
        MaLk = l.MaLk,
        TenLinhKien = l.TenLinhKien,
        HangSanXuat = l.HangSanXuat,
        MoTa = l.MoTa,
        DonViTinh = l.DonViTinh,
        DonGiaBan = l.DonGiaBan,
        ThoiHanBaoHanhThang = l.ThoiHanBaoHanhThang,
        TongTon = tongTon,
        CreatedAt = l.CreatedAt,
        UpdatedAt = l.UpdatedAt
    };
}
