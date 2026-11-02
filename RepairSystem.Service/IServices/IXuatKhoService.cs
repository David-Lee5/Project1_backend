using RepairSystem.Service.DTOs;

namespace RepairSystem.Service.IServices;

public interface IXuatKhoService
{
    /// <summary>
    /// Xuất linh kiện (có thể kèm serial cụ thể) cho một phiếu sửa chữa. Chỉ thủ kho thực hiện.
    /// Trừ tồn, ghi sổ biến động và đổi trạng thái serial trong cùng một transaction.
    /// </summary>
    Task<List<DongXuatKhoDto>> XuatKhoAsync(long phieuId, XuatKhoRequestDto dto, long taiKhoanId);

    /// <summary>Các dòng đã xuất cho một phiếu sửa chữa.</summary>
    Task<List<DongXuatKhoDto>> GetXuatKhoCuaPhieuAsync(long phieuId);
}
