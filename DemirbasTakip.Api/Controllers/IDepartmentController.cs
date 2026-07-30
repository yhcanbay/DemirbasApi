using Microsoft.AspNetCore.Mvc;
using DemirbasTakip.Api.DTOs;

namespace DemirbasTakip.Api.Controllers;

public interface IDepartmentController
{
    Task<IActionResult> GetAll();
    Task<IActionResult> GetById(int id);
    Task<IActionResult> Create([FromBody] CreateDepartmentDto dto);
    Task<IActionResult> Update(int id, [FromBody] UpdateDepartmentDto dto);
    Task<IActionResult> Delete(int id);
    Task<IActionResult> AssignPersonnel([FromBody] AssignPersonnelToDepartmentDto dto);
    Task<IActionResult> RemovePersonnel(int departmentId, int personnelId);
}
