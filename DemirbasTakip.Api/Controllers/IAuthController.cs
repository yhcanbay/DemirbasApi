using DemirbasTakip.Api.DTOs;
using Microsoft.AspNetCore.Mvc;

public interface IAuthController
{
    [HttpPost("login")]
    Task<IActionResult> Login([FromBody] LoginDto dto);

    // [HttpGet("me")]
    // public IActionResult Me();
}