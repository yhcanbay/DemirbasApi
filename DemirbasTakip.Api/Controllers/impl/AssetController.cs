using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DemirbasTakip.Api.Common;
using DemirbasTakip.Api.DTOs;
using DemirbasTakip.Api.Services;

namespace DemirbasTakip.Api.Controllers;

[ApiController]
[Route("assets")]
public class AssetController : ControllerBase, IAssetController
{
    private readonly IAssetService _assetService;

    public AssetController(IAssetService assetService) => _assetService = assetService;

    // GET /api/assets
    // Hem Admin hem User erişebilir.
    [HttpGet]
    [Authorize(Policy = "AllowedUser")]
    public async Task<IActionResult> GetAll()
    {
        var list = await _assetService.GetAllAsync();
        return Ok(ApiResponse<List<AssetResponseDto>>.Ok(list));
    }

    // GET /api/assets/{id}
    // Hem Admin hem User erişebilir.
    [HttpGet("{id:int}")]
    [Authorize(Policy = "AllowedUser")]
    public async Task<IActionResult> GetById(int id)
    {
        var asset = await _assetService.GetByIdAsync(id);

        if (asset is null)
            return NotFound(ApiResponse.Fail("Demirbaş bulunamadı."));

        return Ok(ApiResponse<AssetResponseDto>.Ok(asset));
    }

    // POST /api/assets
    // FallbackPolicy → sadece Admin
    // null = Code zaten kullanımda → 409 Conflict
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateAssetDto dto)
    {
        var result = await _assetService.CreateAsync(dto);

        if (result is null)
            return Conflict(ApiResponse.Fail("Bu envanter kodu (Code) zaten kullanımda."));

        return StatusCode(201, ApiResponse.Ok("Demirbaş oluşturuldu."));
    }

    // PUT /api/assets/{id}
    // FallbackPolicy → sadece Admin
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateAssetDto dto)
    {
        var result = await _assetService.UpdateAsync(id, dto);

        if (!result)
            return NotFound(ApiResponse.Fail("Demirbaş bulunamadı."));

        return Ok(ApiResponse.Ok("Demirbaş güncellendi."));
    }

    // DELETE /api/assets/{id}
    // FallbackPolicy → sadece Admin
    // null = aktif zimmeti var → 409 Conflict
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _assetService.DeleteAsync(id);

        if (result is null)
            return Conflict(ApiResponse.Fail("Demirbaşın aktif zimmeti var. Önce iade edin."));

        if (result == false)
            return NotFound(ApiResponse.Fail("Demirbaş bulunamadı."));

        return Ok(ApiResponse.Ok("Demirbaş silindi."));
    }
}
