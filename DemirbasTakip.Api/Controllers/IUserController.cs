using Microsoft.AspNetCore.Mvc;
using DemirbasTakip.Api.DTOs.Request.Create;

namespace DemirbasTakip.Api.Controllers;

public interface IUserController
{
    Task<IActionResult> GetAll();
    Task<IActionResult> GetById(int id);
    Task<IActionResult> Create([FromBody] CreateUserDto dto);
    Task<IActionResult> Delete(int id);
}
