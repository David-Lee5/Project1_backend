using RepairSystem.Service.DTOs;

namespace RepairSystem.Service.IServices;

public interface IKhoService
{
    Task<IEnumerable<KhoDto>> GetAllKhoAsync();

    /// <param name="khoId">Lọc theo kho (bỏ trống = tất cả kho).</param>
    /// <param name="keyword">Tìm theo mã hoặc tên linh kiện.</param>
    /// <param name="chiSapHet">true: chỉ lấy dòng hết hàng hoặc có tồn dưới mức tối thiểu.</param>
    Task<IEnumerable<TonKhoDto>> GetTonKhoAsync(long? khoId = null, string? keyword = null, bool chiSapHet = false);

    /// <summary>Các linh kiện hết hàng hoặc có tồn dưới mức tối thiểu, hết hàng xếp trước.</summary>
    Task<IEnumerable<CanhBaoTonKhoDto>> GetCanhBaoAsync(long? khoId = null);

    /// <summary>Đặt ngưỡng cảnh báo cho một linh kiện trong một kho.</summary>
    /// <exception cref="KeyNotFoundException">Chưa có dòng tồn kho (linh kiện chưa từng được nhập vào kho đó).</exception>
    Task<TonKhoDto> CapNhatMucTonToiThieuAsync(long khoId, long linhKienId, int mucTonToiThieu);
}
