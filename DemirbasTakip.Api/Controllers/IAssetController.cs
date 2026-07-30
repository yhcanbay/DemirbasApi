using Microsoft.AspNetCore.Mvc;
using DemirbasTakip.Api.DTOs;

namespace DemirbasTakip.Api.Controllers;

public interface IAssetController
{
    Task<IActionResult> GetAll();
    Task<IActionResult> GetById(int id);
    Task<IActionResult> Create([FromBody] CreateAssetDto dto);
    Task<IActionResult> Update(int id, [FromBody] UpdateAssetDto dto);
    Task<IActionResult> Delete(int id);
}
