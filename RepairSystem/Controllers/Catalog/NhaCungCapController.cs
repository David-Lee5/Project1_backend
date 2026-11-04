using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RepairSystem.Service.DTOs;
using RepairSystem.Service.Helpers;
using RepairSystem.Service.IServices;

namespace RepairSystem.Controllers;

[ApiController]
[Route("api/nha-cung-cap")]
[Authorize]
public class NhaCungCapController : ControllerBase
{
    private readonly INhaCungCapService _service;

    public NhaCungCapController(INhaCungCapService service)
    {
        _service = service;
    }

    /// <summary>Lấy danh sách nhà cung cấp.</summary>
    [HttpGet]
    [Authorize(Roles = Roles.NhanVien)]
    public async Task<ActionResult<ApiResponse<IEnumerable<NhaCungCapDto>>>> GetAll(
        [FromQuery] bool includeDeleted = false)
    {
        var result = await _service.GetAllAsync(includeDeleted);
        return Ok(ApiResponse<IEnumerable<NhaCungCapDto>>.SuccessResponse(result));
    }

    /// <summary>Lấy thông tin một nhà cung cấp theo id.</summary>
    [HttpGet("{id:long}")]
    [Authorize(Roles = Roles.NhanVien)]
    public async Task<ActionResult<ApiResponse<NhaCungCapDto>>> GetById(long id)
    {
        var ncc = await _service.GetByIdAsync(id);
        if (ncc == null)
            return NotFound(ApiResponse<NhaCungCapDto>.ErrorResponse($"Không tìm thấy nhà cung cấp id={id}."));

        return Ok(ApiResponse<NhaCungCapDto>.SuccessResponse(ncc));
    }

    /// <summary>Tạo nhà cung cấp mới. Yêu cầu role Admin.</summary>
    [HttpPost]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<ApiResponse<NhaCungCapDto>>> Create([FromBody] UpsertNhaCungCapDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<NhaCungCapDto>.ErrorResponse("Dữ liệu không hợp lệ."));

        var ncc = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = ncc.Id },
            ApiResponse<NhaCungCapDto>.SuccessResponse(ncc, "Tạo nhà cung cấp thành công."));
    }

    /// <summary>Cập nhật nhà cung cấp. Yêu cầu role Admin.</summary>
    [HttpPut("{id:long}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<ApiResponse<NhaCungCapDto>>> Update(long id, [FromBody] UpsertNhaCungCapDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<NhaCungCapDto>.ErrorResponse("Dữ liệu không hợp lệ."));

        try
        {
            var ncc = await _service.UpdateAsync(id, dto);
            return Ok(ApiResponse<NhaCungCapDto>.SuccessResponse(ncc, "Cập nhật nhà cung cấp thành công."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<NhaCungCapDto>.ErrorResponse(ex.Message));
        }
    }

    /// <summary>Xoá mềm nhà cung cấp (đặt deleted_at). Yêu cầu role Admin.</summary>
    [HttpDelete("{id:long}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(long id)
    {
        var result = await _service.DeleteAsync(id);
        if (!result)
            return NotFound(ApiResponse<bool>.ErrorResponse($"Không tìm thấy nhà cung cấp id={id}."));

        return Ok(ApiResponse<bool>.SuccessResponse(true, "Xoá nhà cung cấp thành công."));
    }
}
