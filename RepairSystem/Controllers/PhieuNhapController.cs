using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RepairSystem.Service.DTOs;
using RepairSystem.Service.Helpers;
using RepairSystem.Service.IServices;

namespace RepairSystem.Controllers;

/// <summary>
/// Phiếu nhập kho. Chỉ Admin và Thủ kho truy cập vì có giá vốn (kỹ thuật viên không được xem).
/// </summary>
[ApiController]
[Route("api/phieu-nhap")]
[Authorize(Roles = Roles.Admin + "," + Roles.ThuKho)]
public class PhieuNhapController : ControllerBase
{
    private readonly IPhieuNhapService _service;

    public PhieuNhapController(IPhieuNhapService service)
    {
        _service = service;
    }

    /// <summary>
    /// Tạo phiếu nhập. Mặc định (xacNhanNgay = true) nhập kho luôn: cộng tồn và ghi biến động kho.
    /// Đặt xacNhanNgay = false để chỉ lưu nháp rồi xác nhận sau.
    /// </summary>
    [HttpPost("tao-moi")]
    public async Task<ActionResult<ApiResponse<PhieuNhapChiTietDto>>> TaoMoi([FromBody] TaoPhieuNhapDto dto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<PhieuNhapChiTietDto>.ErrorResponse("Dữ liệu không hợp lệ.", errors));
        }

        var taiKhoanId = User.GetTaiKhoanId();
        if (taiKhoanId == null)
            return Unauthorized(ApiResponse<PhieuNhapChiTietDto>.ErrorResponse("Không xác định được người dùng."));

        var vaiTro = User.IsInRole(Roles.ThuKho) ? Roles.ThuKho : Roles.Admin;

        try
        {
            var result = await _service.TaoMoiAsync(dto, taiKhoanId.Value, vaiTro);
            var message = result.Phieu.TrangThai == "DA_NHAP_KHO"
                ? "Nhập kho thành công, tồn kho đã được cập nhật."
                : "Đã lưu phiếu nhập nháp.";

            return CreatedAtAction(nameof(GetById), new { id = result.Phieu.Id },
                ApiResponse<PhieuNhapChiTietDto>.SuccessResponse(result, message));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<PhieuNhapChiTietDto>.ErrorResponse(ex.Message));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<PhieuNhapChiTietDto>.ErrorResponse(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse<PhieuNhapChiTietDto>.ErrorResponse(ex.Message));
        }
    }

    /// <summary>Xác nhận nhập kho một phiếu nháp (NHAP -> DA_NHAP_KHO), cộng tồn kho.</summary>
    [HttpPost("{id:long}/xac-nhan")]
    public async Task<ActionResult<ApiResponse<PhieuNhapChiTietDto>>> XacNhan(long id)
    {
        var taiKhoanId = User.GetTaiKhoanId();
        if (taiKhoanId == null)
            return Unauthorized(ApiResponse<PhieuNhapChiTietDto>.ErrorResponse("Không xác định được người dùng."));

        try
        {
            var result = await _service.XacNhanAsync(id, taiKhoanId.Value);
            return Ok(ApiResponse<PhieuNhapChiTietDto>.SuccessResponse(result, "Nhập kho thành công, tồn kho đã được cập nhật."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<PhieuNhapChiTietDto>.ErrorResponse(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse<PhieuNhapChiTietDto>.ErrorResponse(ex.Message));
        }
    }

    /// <summary>Huỷ phiếu nháp. Phiếu đã nhập kho không huỷ được.</summary>
    [HttpPost("{id:long}/huy")]
    public async Task<ActionResult<ApiResponse<PhieuNhapChiTietDto>>> Huy(long id)
    {
        try
        {
            var result = await _service.HuyAsync(id);
            return Ok(ApiResponse<PhieuNhapChiTietDto>.SuccessResponse(result, "Đã huỷ phiếu nhập."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<PhieuNhapChiTietDto>.ErrorResponse(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse<PhieuNhapChiTietDto>.ErrorResponse(ex.Message));
        }
    }

    /// <summary>
    /// Danh sách phiếu nhập, mới nhất trước.
    /// Query: trangThai (NHAP | DA_NHAP_KHO | HUY), nhaCungCapId, khoId, tuNgay, denNgay (yyyy-MM-dd).
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<PhieuNhapDto>>>> GetAll(
        [FromQuery] string? trangThai = null,
        [FromQuery] long? nhaCungCapId = null,
        [FromQuery] long? khoId = null,
        [FromQuery] DateTime? tuNgay = null,
        [FromQuery] DateTime? denNgay = null)
    {
        var result = await _service.GetAllAsync(trangThai, nhaCungCapId, khoId, tuNgay, denNgay);
        return Ok(ApiResponse<IEnumerable<PhieuNhapDto>>.SuccessResponse(result));
    }

    /// <summary>Chi tiết một phiếu nhập kèm các dòng linh kiện.</summary>
    [HttpGet("{id:long}")]
    public async Task<ActionResult<ApiResponse<PhieuNhapChiTietDto>>> GetById(long id)
    {
        var result = await _service.GetByIdAsync(id);
        if (result == null)
            return NotFound(ApiResponse<PhieuNhapChiTietDto>.ErrorResponse($"Không tìm thấy phiếu nhập id={id}."));

        return Ok(ApiResponse<PhieuNhapChiTietDto>.SuccessResponse(result));
    }
}
