using RepairSystem.Service.DTOs;

namespace RepairSystem.Service.IServices;

public interface INhanVienService
{
    Task<IEnumerable<NhanVienDto>> GetAllAsync(string? chucVu = null, bool includeDeleted = false);
    Task<NhanVienDto?> GetByIdAsync(long id);
    Task<NhanVienDto> CreateAsync(CreateNhanVienDto dto);
    Task<NhanVienDto> UpdateAsync(long id, UpdateNhanVienDto dto);
    /// <summary>Soft-delete nhân viên (đặt deleted_at).</summary>
    Task<bool> DeleteAsync(long id);
}
