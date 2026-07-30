using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DemirbasTakip.Api.Common;
using DemirbasTakip.Api.DTOs;
using DemirbasTakip.Api.Services;

namespace DemirbasTakip.Api.Controllers;

[ApiController]
[Route("personnel")]
public class PersonnelController : ControllerBase, IPersonnelController
{
    private readonly IPersonnelService _personnelService;

    public PersonnelController(IPersonnelService personnelService)
        => _personnelService = personnelService;

    // GET /api/personnel
    // Hem Admin hem User erişebilir.
    [HttpGet]
    [Authorize(Policy = "AllowedUser")]
    public async Task<IActionResult> GetAll()
    {
        var list = await _personnelService.GetAllAsync();
        return Ok(ApiResponse<List<PersonnelResponseDto>>.Ok(list));
    }

    // GET /api/personnel/{id}
    // Hem Admin hem User erişebilir.
    [HttpGet("{id:int}")]
    [Authorize(Policy = "AllowedUser")]
    public async Task<IActionResult> GetById(int id)
    {
        var personnel = await _personnelService.GetByIdAsync(id);

        if (personnel is null)
            return NotFound(ApiResponse.Fail("Personel bulunamadı."));

        return Ok(ApiResponse<PersonnelResponseDto>.Ok(personnel));
    }

    // PUT /api/personnel/{id}
    // FallbackPolicy → sadece Admin
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdatePersonnelDto dto)
    {
        var result = await _personnelService.UpdateAsync(id, dto);

        if (!result)
            return NotFound(ApiResponse.Fail("Personel bulunamadı."));

        return Ok(ApiResponse.Ok("Personel bilgileri güncellendi."));
    }

    // DELETE /api/personnel/{id}
    // FallbackPolicy → sadece Admin
    // null = aktif zimmeti var → 409 Conflict
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _personnelService.DeleteAsync(id);

        if (result is null)
            return Conflict(ApiResponse.Fail("Personelin aktif zimmeti var. Önce zimmetleri iade edin."));

        if (result == false)
            return NotFound(ApiResponse.Fail("Personel bulunamadı."));

        return Ok(ApiResponse.Ok("Personel silindi."));
    }
}