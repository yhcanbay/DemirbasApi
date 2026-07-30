using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DemirbasTakip.Api.Common;
using DemirbasTakip.Api.DTOs;
using DemirbasTakip.Api.Services;

namespace DemirbasTakip.Api.Controllers;

[ApiController]
[Route("assignments")]
public class AssetAssignmentController : ControllerBase, IAssetAssignmentController
{
    private readonly IAssetAssignmentService _assignmentService;

    public AssetAssignmentController(IAssetAssignmentService assignmentService)
        => _assignmentService = assignmentService;

    // GET /api/assignments
    // Hem Admin hem User erişebilir.
    [HttpGet]
    [Authorize(Policy = "AllowedUser")]
    public async Task<IActionResult> GetAll()
    {
        var list = await _assignmentService.GetAllAsync();
        return Ok(ApiResponse<List<AssetAssignmentResponseDto>>.Ok(list));
    }

    // GET /api/assignments/personnel/{personnelId}
    // Hem Admin hem User erişebilir.
    [HttpGet("personnel/{personnelId:int}")]
    [Authorize(Policy = "AllowedUser")]
    public async Task<IActionResult> GetByPersonnelId(int personnelId)
    {
        var list = await _assignmentService.GetByPersonnelIdAsync(personnelId);
        return Ok(ApiResponse<List<AssetAssignmentResponseDto>>.Ok(list));
    }

    // GET /api/assignments/asset/{assetId}
    // FallbackPolicy → sadece Admin
    [HttpGet("asset/{assetId:int}")]
    public async Task<IActionResult> GetByAssetId(int assetId)
    {
        var list = await _assignmentService.GetByAssetIdAsync(assetId);
        return Ok(ApiResponse<List<AssetAssignmentResponseDto>>.Ok(list));
    }

    // GET /api/assignments/my
    // Hem Admin hem User erişebilir. Token'dan UserId okunur.
    // Personnel kaydı yoksa (Admin gibi) 404 döner.
    [HttpGet("my")]
    [Authorize(Policy = "AllowedUser")]
    public async Task<IActionResult> GetMyAssignments()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userIdClaim is null || !int.TryParse(userIdClaim, out var userId))
            return Unauthorized(ApiResponse.Fail("Geçersiz token."));

        var list = await _assignmentService.GetMyAssignmentsAsync(userId);

        if (list is null)
            return NotFound(ApiResponse.Fail("Bu kullanıcıya ait personel kaydı bulunamadı."));

        return Ok(ApiResponse<List<AssetAssignmentResponseDto>>.Ok(list));
    }

    // POST /api/assignments
    // FallbackPolicy → sadece Admin
    [HttpPost]
    public async Task<IActionResult> Assign([FromBody] CreateAssetAssignmentDto dto)
    {
        var result = await _assignmentService.AssignAsync(dto);

        if (result is null)
            return Conflict(ApiResponse.Fail("Demirbaş zaten başka bir personelde aktif olarak zimmette."));

        if (result == false)
            return NotFound(ApiResponse.Fail("Demirbaş veya personel bulunamadı."));

        return StatusCode(201, ApiResponse.Ok("Demirbaş zimmeti oluşturuldu."));
    }

    // PATCH /api/assignments/{id}/return
    // FallbackPolicy → sadece Admin
    [HttpPatch("{id:int}/return")]
    public async Task<IActionResult> Return(int id)
    {
        var result = await _assignmentService.ReturnAsync(id);

        if (!result)
            return NotFound(ApiResponse.Fail("Zimmet bulunamadı veya zaten iade edilmiş."));

        return Ok(ApiResponse.Ok("Zimmet iade edildi."));
    }
}
