using Microsoft.AspNetCore.Mvc;
using DemirbasTakip.Api.DTOs;

namespace DemirbasTakip.Api.Controllers;

public interface IAssetAssignmentController
{
    Task<IActionResult> GetAll();
    Task<IActionResult> GetByPersonnelId(int personnelId);
    Task<IActionResult> GetByAssetId(int assetId);
    Task<IActionResult> GetMyAssignments();
    Task<IActionResult> Assign([FromBody] CreateAssetAssignmentDto dto);
    Task<IActionResult> Return(int id);
}
