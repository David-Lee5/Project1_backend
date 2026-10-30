using RepairSystem.Service.DTOs;

namespace RepairSystem.Service.IServices;

public interface IPhieuNhapService
{
    /// <summary>
    /// Tạo phiếu nhập. Nếu <see cref="TaoPhieuNhapDto.XacNhanNgay"/> = true thì nhập kho luôn
    /// (cộng tồn và ghi biến động kho) trong cùng một transaction.
    /// </summary>
    /// <exception cref="ArgumentException">Dữ liệu không hợp lệ (NCC/kho/linh kiện không tồn tại, dòng trùng...).</exception>
    /// <exception cref="InvalidOperationException">Vi phạm trạng thái hoặc ràng buộc dữ liệu.</exception>
    Task<PhieuNhapChiTietDto> TaoMoiAsync(TaoPhieuNhapDto dto, long taiKhoanId, string vaiTro);

    /// <summary>Xác nhận nhập kho một phiếu nháp (NHAP -> DA_NHAP_KHO), cộng tồn kho.</summary>
    /// <exception cref="KeyNotFoundException">Không tìm thấy phiếu.</exception>
    /// <exception cref="InvalidOperationException">Phiếu không ở trạng thái NHAP hoặc chưa có chi tiết.</exception>
    Task<PhieuNhapChiTietDto> XacNhanAsync(long id, long taiKhoanId);

    /// <summary>Huỷ phiếu nháp (NHAP -> HUY). Phiếu đã nhập kho không huỷ được.</summary>
    Task<PhieuNhapChiTietDto> HuyAsync(long id);

    Task<IEnumerable<PhieuNhapDto>> GetAllAsync(
        string? trangThai = null, long? nhaCungCapId = null, long? khoId = null,
        DateTime? tuNgay = null, DateTime? denNgay = null);

    Task<PhieuNhapChiTietDto?> GetByIdAsync(long id);
}
