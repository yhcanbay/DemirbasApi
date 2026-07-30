using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DemirbasTakip.Api.Common;
using DemirbasTakip.Api.DTOs;
using DemirbasTakip.Api.Services;

namespace DemirbasTakip.Api.Controllers;

[ApiController]
[Route("departments")]
public class DepartmentController : ControllerBase, IDepartmentController
{
    private readonly IDepartmentService _departmentService;

    public DepartmentController(IDepartmentService departmentService)
        => _departmentService = departmentService;

    // GET /api/departments
    // Hem Admin hem User erişebilir.
    [HttpGet]
    [Authorize(Policy = "AllowedUser")]
    public async Task<IActionResult> GetAll()
    {
        var list = await _departmentService.GetAllAsync();
        return Ok(ApiResponse<List<DepartmentResponseDto>>.Ok(list));
    }

    // GET /api/departments/{id}
    // Hem Admin hem User erişebilir.
    [HttpGet("{id:int}")]
    [Authorize(Policy = "AllowedUser")]
    public async Task<IActionResult> GetById(int id)
    {
        var dept = await _departmentService.GetByIdAsync(id);

        if (dept is null)
            return NotFound(ApiResponse.Fail("Departman bulunamadı."));

        return Ok(ApiResponse<DepartmentResponseDto>.Ok(dept));
    }

    // POST /api/departments
    // FallbackPolicy → sadece Admin
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateDepartmentDto dto)
    {
        var newId = await _departmentService.CreateAsync(dto);
        return StatusCode(201, ApiResponse<object>.Ok(new { id = newId }, "Departman oluşturuldu."));
    }

    // PUT /api/departments/{id}
    // FallbackPolicy → sadece Admin
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateDepartmentDto dto)
    {
        var result = await _departmentService.UpdateAsync(id, dto);

        if (!result)
            return NotFound(ApiResponse.Fail("Departman bulunamadı."));

        return Ok(ApiResponse.Ok("Departman güncellendi."));
    }

    // DELETE /api/departments/{id}
    // FallbackPolicy → sadece Admin
    // null = aktif personeli var → 409 Conflict
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _departmentService.DeleteAsync(id);

        if (result is null)
            return Conflict(ApiResponse.Fail("Departmanda aktif personel var. Önce personeli çıkarın."));

        if (result == false)
            return NotFound(ApiResponse.Fail("Departman bulunamadı."));

        return Ok(ApiResponse.Ok("Departman silindi."));
    }

    // POST /api/departments/assign-personnel
    // FallbackPolicy → sadece Admin
    [HttpPost("assign-personnel")]
    public async Task<IActionResult> AssignPersonnel([FromBody] AssignPersonnelToDepartmentDto dto)
    {
        var result = await _departmentService.AssignPersonnelAsync(dto);

        if (result is null)
            return NotFound(ApiResponse.Fail("Personel veya departman bulunamadı."));

        if (result == false)
            return Conflict(ApiResponse.Fail("Personel zaten bu departmanda aktif olarak kayıtlı."));

        return StatusCode(201, ApiResponse.Ok("Personel departmana atandı."));
    }

    // PATCH /api/departments/{departmentId}/personnel/{personnelId}/leave
    // FallbackPolicy → sadece Admin
    [HttpPatch("{departmentId:int}/personnel/{personnelId:int}/leave")]
    public async Task<IActionResult> RemovePersonnel(int departmentId, int personnelId)
    {
        var result = await _departmentService.RemovePersonnelAsync(departmentId, personnelId);

        if (!result)
            return NotFound(ApiResponse.Fail("Bu personele ait aktif departman ataması bulunamadı."));

        return Ok(ApiResponse.Ok("Personel departmandan çıkarıldı."));
    }
}
