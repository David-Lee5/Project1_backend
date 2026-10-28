using Microsoft.EntityFrameworkCore;
using RepairSystem.Model;
using RepairSystem.Service.DTOs;
using RepairSystem.Service.IServices;

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
        var query = _context.NhaCungCaps.AsQueryable();

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
        var ncc = new NhaCungCap
        {
            TenNcc = dto.TenNcc,
            MaSoThue = dto.MaSoThue,
            SoDienThoai = dto.SoDienThoai,
            Email = dto.Email,
            DiaChi = dto.DiaChi
        };

        _context.NhaCungCaps.Add(ncc);
        await _context.SaveChangesAsync();
        return ToDto(ncc);
    }

    public async Task<NhaCungCapDto> UpdateAsync(long id, UpsertNhaCungCapDto dto)
    {
        var ncc = await _context.NhaCungCaps
            .FirstOrDefaultAsync(n => n.Id == id && n.DeletedAt == null)
            ?? throw new KeyNotFoundException($"Không tìm thấy nhà cung cấp id={id}.");

        ncc.TenNcc = dto.TenNcc;
        ncc.MaSoThue = dto.MaSoThue;
        ncc.SoDienThoai = dto.SoDienThoai;
        ncc.Email = dto.Email;
        ncc.DiaChi = dto.DiaChi;
        ncc.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return ToDto(ncc);
    }

    public async Task<bool> DeleteAsync(long id)
    {
        var ncc = await _context.NhaCungCaps
            .FirstOrDefaultAsync(n => n.Id == id && n.DeletedAt == null);

        if (ncc == null) return false;

        ncc.DeletedAt = DateTime.UtcNow;
        ncc.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
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
