using Microsoft.EntityFrameworkCore;
using RepairSystem.Model;
using RepairSystem.Service.DTOs;
using RepairSystem.Service.IServices;

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

        var list = await Project(query.OrderBy(l => l.TenLinhKien)).ToListAsync();

        if (!includeStock)
            list.ForEach(x => x.TongTon = null);

        return list;
    }

    public async Task<LinhKienDto?> GetByIdAsync(long id, bool includeStock = false)
    {
        var dto = await Project(_context.LinhKiens.AsNoTracking()
                .Where(l => l.Id == id && l.DeletedAt == null))
            .FirstOrDefaultAsync();

        if (dto != null && !includeStock)
            dto.TongTon = null;

        return dto;
    }

    public async Task<LinhKienDto> CreateAsync(UpsertLinhKienDto dto)
    {
        var lk = new LinhKien
        {
            TenLinhKien = dto.TenLinhKien.Trim(),
            HangSanXuat = NullIfBlank(dto.HangSanXuat),
            MoTa = NullIfBlank(dto.MoTa),
            DonViTinh = dto.DonViTinh.Trim(),
            DonGiaBan = dto.DonGiaBan,
            ThoiHanBaoHanhThang = (short)dto.ThoiHanBaoHanhThang
        };

        _context.LinhKiens.Add(lk);
        await _context.SaveChangesAsync();

        // Linh kiện mới chưa nhập kho nên tồn = 0. Dòng ton_kho sẽ tự được tạo khi xác nhận phiếu nhập đầu tiên.
        return ToDto(lk, tongTon: 0);
    }

    public async Task<LinhKienDto> UpdateAsync(long id, UpsertLinhKienDto dto)
    {
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

        var tongTon = await _context.TonKhos.Where(t => t.LinhKienId == id).SumAsync(t => t.SoLuongTon);
        return ToDto(lk, tongTon);
    }

    public async Task<bool> DeleteAsync(long id)
    {
        var lk = await _context.LinhKiens
            .FirstOrDefaultAsync(l => l.Id == id && l.DeletedAt == null);

        if (lk == null) return false;

        lk.DeletedAt = DateTime.UtcNow;
        lk.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    // ── helpers ────────────────────────────────────────────────────
    private static IQueryable<LinhKienDto> Project(IQueryable<LinhKien> query) =>
        query.Select(l => new LinhKienDto
        {
            Id = l.Id,
            MaLk = l.MaLk,
            TenLinhKien = l.TenLinhKien,
            HangSanXuat = l.HangSanXuat,
            MoTa = l.MoTa,
            DonViTinh = l.DonViTinh,
            DonGiaBan = l.DonGiaBan,
            ThoiHanBaoHanhThang = l.ThoiHanBaoHanhThang,
            TongTon = l.TonKhos.Sum(t => t.SoLuongTon),
            CreatedAt = l.CreatedAt,
            UpdatedAt = l.UpdatedAt
        });

    private static string? NullIfBlank(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static LinhKienDto ToDto(LinhKien l, int tongTon) => new()
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
