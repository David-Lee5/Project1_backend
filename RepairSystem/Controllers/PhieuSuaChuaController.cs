using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RepairSystem.Service.DTOs;
using RepairSystem.Service.Helpers;
using RepairSystem.Service.Services;

namespace RepairSystem.Controllers;

[ApiController]
[Route("api/phieu-sua-chua")]
[Authorize]
public class PhieuSuaChuaController : ControllerBase
{
    private readonly RepairService _service;
    public PhieuSuaChuaController(RepairService service) => _service = service;

    private long AccountId => long.TryParse(User.FindFirstValue("UserId") ?? User.FindFirstValue(ClaimTypes.NameIdentifier), out var id)
        ? id : throw new UnauthorizedAccessException("Không xác định được tài khoản.");

    [HttpGet]
    public async Task<ActionResult<ApiResponse<RepairPageDto>>> GetAll([FromQuery] string? search = null,
        [FromQuery] string? trangThai = null, [FromQuery] long? kyThuatVienId = null,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20) =>
        Ok(ApiResponse<RepairPageDto>.SuccessResponse(await _service.GetAllAsync(AccountId, search, trangThai, kyThuatVienId, page, pageSize)));

    [HttpGet("{id:long}")]
    public async Task<ActionResult<ApiResponse<RepairDetailDto>>> GetById(long id) =>
        Ok(ApiResponse<RepairDetailDto>.SuccessResponse(await _service.GetByIdAsync(AccountId, id)));

    [HttpPost]
    [Authorize(Roles = Roles.Admin + "," + Roles.LeTan)]
    public async Task<ActionResult<ApiResponse<RepairDetailDto>>> Create(CreateRepairDto dto)
    {
        var result = await _service.CreateAsync(AccountId, dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Phieu.Id }, ApiResponse<RepairDetailDto>.SuccessResponse(result, "Tiếp nhận phiếu thành công."));
    }

    [HttpPut("{id:long}")]
    [Authorize(Roles = Roles.Admin + "," + Roles.LeTan)]
    public async Task<ActionResult<ApiResponse<RepairDetailDto>>> Update(long id, UpdateRepairDto dto) =>
        Ok(ApiResponse<RepairDetailDto>.SuccessResponse(await _service.UpdateAsync(AccountId, id, dto), "Cập nhật phiếu thành công."));

    [HttpPut("{id:long}/phan-cong")]
    [Authorize(Roles = Roles.Admin + "," + Roles.LeTan)]
    public async Task<ActionResult<ApiResponse<RepairDetailDto>>> Assign(long id, AssignRepairDto dto) =>
        Ok(ApiResponse<RepairDetailDto>.SuccessResponse(await _service.AssignAsync(AccountId, id, dto), "Phân công kỹ thuật viên thành công."));

    [HttpPut("{id:long}/trang-thai")]
    [Authorize(Roles = Roles.Admin + "," + Roles.LeTan + "," + Roles.KyThuatVien)]
    public async Task<ActionResult<ApiResponse<RepairDetailDto>>> ChangeStatus(long id, ChangeRepairStatusDto dto) =>
        Ok(ApiResponse<RepairDetailDto>.SuccessResponse(await _service.ChangeStatusAsync(AccountId, id, dto), "Cập nhật trạng thái thành công."));

    [HttpPut("{id:long}/dich-vu/{dichVuId:long}")]
    [Authorize(Roles = Roles.Admin + "," + Roles.LeTan + "," + Roles.KyThuatVien)]
    public async Task<ActionResult<ApiResponse<RepairDetailDto>>> UpsertService(long id, long dichVuId, UpsertRepairServiceDto dto) =>
        Ok(ApiResponse<RepairDetailDto>.SuccessResponse(await _service.UpsertServiceAsync(AccountId, id, dichVuId, dto)));

    [HttpDelete("{id:long}/dich-vu/{dichVuId:long}")]
    [Authorize(Roles = Roles.Admin + "," + Roles.LeTan + "," + Roles.KyThuatVien)]
    public async Task<ActionResult<ApiResponse<RepairDetailDto>>> RemoveService(long id, long dichVuId) =>
        Ok(ApiResponse<RepairDetailDto>.SuccessResponse(await _service.RemoveServiceAsync(AccountId, id, dichVuId)));

    [HttpPost("{id:long}/hinh-anh")]
    [Authorize(Roles = Roles.Admin + "," + Roles.LeTan + "," + Roles.KyThuatVien)]
    public async Task<ActionResult<ApiResponse<RepairDetailDto>>> AddPhoto(long id, AddRepairPhotoDto dto) =>
        Ok(ApiResponse<RepairDetailDto>.SuccessResponse(await _service.AddPhotoAsync(AccountId, id, dto)));

    [HttpDelete("{id:long}/hinh-anh/{photoId:long}")]
    [Authorize(Roles = Roles.Admin + "," + Roles.LeTan + "," + Roles.KyThuatVien)]
    public async Task<ActionResult<ApiResponse<RepairDetailDto>>> RemovePhoto(long id, long photoId) =>
        Ok(ApiResponse<RepairDetailDto>.SuccessResponse(await _service.RemovePhotoAsync(AccountId, id, photoId)));
}
