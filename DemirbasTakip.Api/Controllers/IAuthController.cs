using DemirbasTakip.Api.DTOs;
using Microsoft.AspNetCore.Mvc;

public interface IAuthController
{
    [HttpPost("login")]
    Task<IActionResult> Login([FromBody] LoginDto dto);

    [HttpPost("register")]
    Task<IActionResult> Register([FromBody] RegisterDto dto);

    // [HttpGet("me")]
    // public IActionResult Me();
}