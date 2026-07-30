using Microsoft.AspNetCore.Mvc;
using DemirbasTakip.Api.DTOs;

namespace DemirbasTakip.Api.Controllers;

public interface IPersonnelController
{
    Task<IActionResult> GetAll();
    Task<IActionResult> GetById(int id);
    Task<IActionResult> Update(int id, [FromBody] UpdatePersonnelDto dto);
    Task<IActionResult> Delete(int id);
}