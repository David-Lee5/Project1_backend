using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RepairSystem.Service.DTOs;
using RepairSystem.Service.Helpers;
using RepairSystem.Service.IServices;

namespace RepairSystem.Controllers;

[ApiController]
[Route("api/dich-vu")]
[Authorize]
public class DichVuController : ControllerBase
{
    private readonly IDichVuService _service;

    public DichVuController(IDichVuService service)
    {
        _service = service;
    }

    /// <summary>Lấy danh sách dịch vụ.</summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<IEnumerable<DichVuDto>>>> GetAll(
        [FromQuery] bool includeDeleted = false)
    {
        var result = await _service.GetAllAsync(includeDeleted && User.IsInRole(Roles.Admin));
        return Ok(ApiResponse<IEnumerable<DichVuDto>>.SuccessResponse(result));
    }

    /// <summary>Lấy thông tin một dịch vụ theo id.</summary>
    [HttpGet("{id:long}")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<DichVuDto>>> GetById(long id)
    {
        var dv = await _service.GetByIdAsync(id);
        if (dv == null)
            return NotFound(ApiResponse<DichVuDto>.ErrorResponse($"Không tìm thấy dịch vụ id={id}."));

        return Ok(ApiResponse<DichVuDto>.SuccessResponse(dv));
    }

    /// <summary>Tạo dịch vụ mới. Yêu cầu role Admin.</summary>
    [HttpPost]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<ApiResponse<DichVuDto>>> Create([FromBody] UpsertDichVuDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<DichVuDto>.ErrorResponse("Dữ liệu không hợp lệ."));

        var dv = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = dv.Id },
            ApiResponse<DichVuDto>.SuccessResponse(dv, "Tạo dịch vụ thành công."));
    }

    /// <summary>Cập nhật dịch vụ. Yêu cầu role Admin.</summary>
    [HttpPut("{id:long}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<ApiResponse<DichVuDto>>> Update(long id, [FromBody] UpsertDichVuDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<DichVuDto>.ErrorResponse("Dữ liệu không hợp lệ."));

        try
        {
            var dv = await _service.UpdateAsync(id, dto);
            return Ok(ApiResponse<DichVuDto>.SuccessResponse(dv, "Cập nhật dịch vụ thành công."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<DichVuDto>.ErrorResponse(ex.Message));
        }
    }

    /// <summary>Xoá mềm dịch vụ (đặt deleted_at). Yêu cầu role Admin.</summary>
    [HttpDelete("{id:long}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(long id)
    {
        var result = await _service.DeleteAsync(id);
        if (!result)
            return NotFound(ApiResponse<bool>.ErrorResponse($"Không tìm thấy dịch vụ id={id}."));

        return Ok(ApiResponse<bool>.SuccessResponse(true, "Xoá dịch vụ thành công."));
    }
}
