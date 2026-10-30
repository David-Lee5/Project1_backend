using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RepairSystem.Service.DTOs;
using RepairSystem.Service.Helpers;
using RepairSystem.Service.Services;

namespace RepairSystem.Controllers;

[ApiController, Route("api/kho")]
[Authorize(Roles = Roles.NhanVien)]
public class KhoController : ControllerBase
{
    private readonly InventoryService _service;
    public KhoController(InventoryService service) => _service = service;

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<WarehouseDto>>>> GetAll() =>
        Ok(ApiResponse<List<WarehouseDto>>.SuccessResponse(await _service.GetWarehousesAsync()));

    [HttpGet("{id:long}")]
    public async Task<ActionResult<ApiResponse<WarehouseDto>>> GetById(long id) =>
        Ok(ApiResponse<WarehouseDto>.SuccessResponse(await _service.GetWarehouseAsync(id)));

    [HttpPost, Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<ApiResponse<WarehouseDto>>> Create(UpsertWarehouseDto dto)
    {
        var result = await _service.SaveWarehouseAsync(null, dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<WarehouseDto>.SuccessResponse(result));
    }

    [HttpPut("{id:long}"), Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<ApiResponse<WarehouseDto>>> Update(long id, UpsertWarehouseDto dto) =>
        Ok(ApiResponse<WarehouseDto>.SuccessResponse(await _service.SaveWarehouseAsync(id, dto)));

    [HttpDelete("{id:long}"), Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(long id)
    {
        await _service.DeleteWarehouseAsync(id);
        return Ok(ApiResponse<bool>.SuccessResponse(true));
    }
}
