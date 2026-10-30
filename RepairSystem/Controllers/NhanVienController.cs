using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RepairSystem.Service.DTOs;
using RepairSystem.Service.Helpers;
using RepairSystem.Service.IServices;

namespace RepairSystem.Controllers;

[ApiController]
[Route("api/nhan-vien")]
[Authorize]
public class NhanVienController : ControllerBase
{
    private readonly INhanVienService _service;

    public NhanVienController(INhanVienService service)
    {
        _service = service;
    }

    /// <summary>Lấy danh sách nhân viên. Query: chucVu (tuỳ chọn), includeDeleted (default false).</summary>
    [HttpGet]
    [Authorize(Roles = Roles.NhanVien)]
    public async Task<ActionResult<ApiResponse<IEnumerable<NhanVienDto>>>> GetAll(
        [FromQuery] string? chucVu = null,
        [FromQuery] bool includeDeleted = false)
    {
        var result = await _service.GetAllAsync(chucVu, includeDeleted && User.IsInRole(Roles.Admin));
        return Ok(ApiResponse<IEnumerable<NhanVienDto>>.SuccessResponse(result));
    }

    /// <summary>Lấy thông tin một nhân viên theo id.</summary>
    [HttpGet("{id:long}")]
    [Authorize(Roles = Roles.NhanVien)]
    public async Task<ActionResult<ApiResponse<NhanVienDto>>> GetById(long id)
    {
        var nv = await _service.GetByIdAsync(id);
        if (nv == null)
            return NotFound(ApiResponse<NhanVienDto>.ErrorResponse($"Không tìm thấy nhân viên id={id}."));

        return Ok(ApiResponse<NhanVienDto>.SuccessResponse(nv));
    }

    /// <summary>Tạo nhân viên mới (đồng thời tạo tài khoản).</summary>
    [HttpPost]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<ApiResponse<NhanVienDto>>> Create([FromBody] CreateNhanVienDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<NhanVienDto>.ErrorResponse("Dữ liệu không hợp lệ."));

        try
        {
            var nv = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = nv.Id },
                ApiResponse<NhanVienDto>.SuccessResponse(nv, "Tạo nhân viên thành công."));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse<NhanVienDto>.ErrorResponse(ex.Message));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<NhanVienDto>.ErrorResponse(ex.Message));
        }
    }

    /// <summary>Cập nhật thông tin nhân viên.</summary>
    [HttpPut("{id:long}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<ApiResponse<NhanVienDto>>> Update(long id, [FromBody] UpdateNhanVienDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<NhanVienDto>.ErrorResponse("Dữ liệu không hợp lệ."));

        try
        {
            var nv = await _service.UpdateAsync(id, dto);
            return Ok(ApiResponse<NhanVienDto>.SuccessResponse(nv, "Cập nhật nhân viên thành công."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<NhanVienDto>.ErrorResponse(ex.Message));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<NhanVienDto>.ErrorResponse(ex.Message));
        }
    }

    /// <summary>Xoá mềm nhân viên (đặt deleted_at, vô hiệu hoá tài khoản).</summary>
    [HttpDelete("{id:long}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(long id)
    {
        var result = await _service.DeleteAsync(id);
        if (!result)
            return NotFound(ApiResponse<bool>.ErrorResponse($"Không tìm thấy nhân viên id={id}."));

        return Ok(ApiResponse<bool>.SuccessResponse(true, "Xoá nhân viên thành công."));
    }
}
