using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RepairSystem.Service.DTOs;
using RepairSystem.Service.Helpers;
using RepairSystem.Service.IServices;

namespace RepairSystem.Controllers;

[ApiController]
[Route("api/linh-kien")]
[Authorize]
public class LinhKienController : ControllerBase
{
    /// <summary>Admin và Thủ kho được quản lý danh mục linh kiện.</summary>
    private const string WriteRoles = Roles.Admin + "," + Roles.ThuKho;

    private readonly ILinhKienService _service;

    public LinhKienController(ILinhKienService service)
    {
        _service = service;
    }

    /// <summary>Đã đăng nhập bằng tài khoản nhân viên (khách và khách vãng lai không thấy tồn kho).</summary>
    private bool IsStaff =>
        User.Identity?.IsAuthenticated == true && !User.IsInRole(Roles.KhachHang);

    /// <summary>Lấy danh sách linh kiện (công khai để khách xem giá). Query: keyword, includeDeleted (chỉ Admin).</summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<IEnumerable<LinhKienDto>>>> GetAll(
        [FromQuery] string? keyword = null,
        [FromQuery] bool includeDeleted = false)
    {
        var result = await _service.GetAllAsync(
            keyword,
            includeStock: IsStaff,
            includeDeleted: includeDeleted && User.IsInRole(Roles.Admin));

        return Ok(ApiResponse<IEnumerable<LinhKienDto>>.SuccessResponse(result));
    }

    /// <summary>Lấy thông tin một linh kiện theo id.</summary>
    [HttpGet("{id:long}")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<LinhKienDto>>> GetById(long id)
    {
        var lk = await _service.GetByIdAsync(id, includeStock: IsStaff);
        if (lk == null)
            return NotFound(ApiResponse<LinhKienDto>.ErrorResponse($"Không tìm thấy linh kiện id={id}."));

        return Ok(ApiResponse<LinhKienDto>.SuccessResponse(lk));
    }

    /// <summary>Tạo linh kiện mới. Yêu cầu role Admin hoặc Thủ kho.</summary>
    [HttpPost]
    [Authorize(Roles = WriteRoles)]
    public async Task<ActionResult<ApiResponse<LinhKienDto>>> Create([FromBody] UpsertLinhKienDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<LinhKienDto>.ErrorResponse("Dữ liệu không hợp lệ."));

        var lk = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = lk.Id },
            ApiResponse<LinhKienDto>.SuccessResponse(lk, "Tạo linh kiện thành công."));
    }

    /// <summary>Cập nhật linh kiện. Yêu cầu role Admin hoặc Thủ kho.</summary>
    [HttpPut("{id:long}")]
    [Authorize(Roles = WriteRoles)]
    public async Task<ActionResult<ApiResponse<LinhKienDto>>> Update(long id, [FromBody] UpsertLinhKienDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<LinhKienDto>.ErrorResponse("Dữ liệu không hợp lệ."));

        try
        {
            var lk = await _service.UpdateAsync(id, dto);
            return Ok(ApiResponse<LinhKienDto>.SuccessResponse(lk, "Cập nhật linh kiện thành công."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<LinhKienDto>.ErrorResponse(ex.Message));
        }
    }

    /// <summary>Xoá mềm linh kiện (đặt deleted_at). Yêu cầu role Admin hoặc Thủ kho.</summary>
    [HttpDelete("{id:long}")]
    [Authorize(Roles = WriteRoles)]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(long id)
    {
        var result = await _service.DeleteAsync(id);
        if (!result)
            return NotFound(ApiResponse<bool>.ErrorResponse($"Không tìm thấy linh kiện id={id}."));

        return Ok(ApiResponse<bool>.SuccessResponse(true, "Xoá linh kiện thành công."));
    }
}
