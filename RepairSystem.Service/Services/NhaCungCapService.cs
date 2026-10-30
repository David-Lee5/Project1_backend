using Microsoft.EntityFrameworkCore;
using RepairSystem.Model;
using RepairSystem.Service.DTOs;
using RepairSystem.Service.IServices;
using RepairSystem.Service.Helpers;

namespace RepairSystem.Service.Services;

public class NhaCungCapService : INhaCungCapService
{
    private readonly AppDbContext _context;

    public NhaCungCapService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<NhaCungCapDto>> GetAllAsync(bool includeDeleted = false)
    {
        var query = _context.NhaCungCaps.AsNoTracking().AsQueryable();

        if (!includeDeleted)
            query = query.Where(ncc => ncc.DeletedAt == null);

        var list = await query.OrderBy(ncc => ncc.TenNcc).ToListAsync();
        return list.Select(ToDto);
    }

    public async Task<NhaCungCapDto?> GetByIdAsync(long id)
    {
        var ncc = await _context.NhaCungCaps
            .FirstOrDefaultAsync(n => n.Id == id && n.DeletedAt == null);

        return ncc == null ? null : ToDto(ncc);
    }

    public async Task<NhaCungCapDto> CreateAsync(UpsertNhaCungCapDto dto)
    {
        CatalogValidation.Validate(dto);
        var ncc = new NhaCungCap
        {
            TenNcc = dto.TenNcc.Trim(),
            MaSoThue = CatalogValidation.NullIfBlank(dto.MaSoThue),
            SoDienThoai = CatalogValidation.NullIfBlank(dto.SoDienThoai),
            Email = CatalogValidation.NullIfBlank(dto.Email),
            DiaChi = CatalogValidation.NullIfBlank(dto.DiaChi),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.NhaCungCaps.Add(ncc);
        await _context.SaveChangesAsync();
        return ToDto(ncc);
    }

    public async Task<NhaCungCapDto> UpdateAsync(long id, UpsertNhaCungCapDto dto)
    {
        CatalogValidation.Validate(dto);
        var ncc = await _context.NhaCungCaps
            .FirstOrDefaultAsync(n => n.Id == id && n.DeletedAt == null)
            ?? throw new KeyNotFoundException($"Không tìm thấy nhà cung cấp id={id}.");

        ncc.TenNcc = dto.TenNcc.Trim();
        ncc.MaSoThue = CatalogValidation.NullIfBlank(dto.MaSoThue);
        ncc.SoDienThoai = CatalogValidation.NullIfBlank(dto.SoDienThoai);
        ncc.Email = CatalogValidation.NullIfBlank(dto.Email);
        ncc.DiaChi = CatalogValidation.NullIfBlank(dto.DiaChi);
        ncc.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return ToDto(ncc);
    }

    public async Task<bool> DeleteAsync(long id)
    {
        await using var tx = _context.Database.IsRelational()
            ? await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable)
            : null;
        var ncc = await _context.NhaCungCaps
            .FirstOrDefaultAsync(n => n.Id == id && n.DeletedAt == null);

        if (ncc == null) return false;

        if (await _context.PhieuNhaps.AnyAsync(p => p.NhaCungCapId == id && p.TrangThai == "NHAP"))
            throw new InvalidOperationException("Nhà cung cấp còn phiếu nhập nháp. Hãy xác nhận hoặc huỷ phiếu trước khi xoá.");

        ncc.DeletedAt = DateTime.UtcNow;
        ncc.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        if (tx != null) await tx.CommitAsync();
        return true;
    }

    // ── helper ─────────────────────────────────────────────────────
    private static NhaCungCapDto ToDto(NhaCungCap ncc) => new()
    {
        Id = ncc.Id,
        MaNcc = ncc.MaNcc,
        TenNcc = ncc.TenNcc,
        MaSoThue = ncc.MaSoThue,
        SoDienThoai = ncc.SoDienThoai,
        Email = ncc.Email,
        DiaChi = ncc.DiaChi,
        CreatedAt = ncc.CreatedAt,
        UpdatedAt = ncc.UpdatedAt
    };
}
