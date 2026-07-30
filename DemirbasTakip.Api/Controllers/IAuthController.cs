using DemirbasTakip.Api.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace DemirbasTakip.Api.Controllers;

public interface IAuthController
{
    Task<IActionResult> Login([FromBody] LoginDto dto);
    Task<IActionResult> Register([FromBody] RegisterDto dto);
    Task<IActionResult> Refresh([FromBody] RefreshTokenRequestDto dto);
    Task<IActionResult> Logout();
    IActionResult Me();
}