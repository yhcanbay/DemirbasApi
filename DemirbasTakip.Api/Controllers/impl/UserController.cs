using Microsoft.AspNetCore.Mvc;
using DemirbasTakip.Api.Common;
using DemirbasTakip.Api.DTOs;
using DemirbasTakip.Api.DTOs.Request.Create;
using DemirbasTakip.Api.Services;

namespace DemirbasTakip.Api.Controllers;

// Tüm endpoint'lere FallbackPolicy uygulanır → sadece Admin erişebilir.
[ApiController]
[Route("users")]
public class UserController : ControllerBase, IUserController
{
    private readonly IUserService _userService;

    public UserController(IUserService userService) => _userService = userService;

    // GET /api/users
    // FallbackPolicy → sadece Admin
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var users = await _userService.GetAllAsync();
        return Ok(ApiResponse<List<UserResponseDto>>.Ok(users));
    }

    // GET /api/users/{id}
    // FallbackPolicy → sadece Admin
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var user = await _userService.GetByIdAsync(id);

        if (user is null)
            return NotFound(ApiResponse.Fail("Kullanıcı bulunamadı."));

        return Ok(ApiResponse<UserResponseDto>.Ok(user));
    }

    // POST /api/users
    // FallbackPolicy → sadece Admin
    // Tek istekte hem User hem Personnel oluşturulur.
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserDto dto)
    {
        var result = await _userService.CreateAsync(dto);

        if (result is null)
            return Conflict(ApiResponse.Fail("Bu kullanıcı adı zaten alınmış."));

        if (result == false)
            return BadRequest(ApiResponse.Fail("Geçersiz rol Id'si. 1=Admin, 2=User olmalıdır."));

        return StatusCode(201, ApiResponse.Ok("Kullanıcı ve personel kaydı başarıyla oluşturuldu."));
    }

    // DELETE /api/users/{id}
    // FallbackPolicy → sadece Admin
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _userService.DeleteAsync(id);

        if (!result)
            return NotFound(ApiResponse.Fail("Kullanıcı bulunamadı."));

        return Ok(ApiResponse.Ok("Kullanıcı ve personel kaydı silindi."));
    }
}
