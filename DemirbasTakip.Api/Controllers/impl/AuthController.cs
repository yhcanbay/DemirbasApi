using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using DemirbasTakip.Api.Common;
using DemirbasTakip.Api.DTOs;
using DemirbasTakip.Api.Services;

namespace DemirbasTakip.Api.Controllers;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase, IAuthController
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService) => _authService = authService;

    // POST /api/auth/register
    // FallbackPolicy → sadece Admin çağırabilir.
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        var result = await _authService.RegisterAsync(dto.Username, dto.Password, dto.RoleName);

        if (result is null)
            return Conflict(ApiResponse.Fail("Bu kullanıcı adı zaten alınmış."));

        if (result == false)
            return BadRequest(ApiResponse.Fail("Geçersiz rol adı. 'Admin' veya 'User' olmalıdır."));

        return StatusCode(201, ApiResponse.Ok("Kullanıcı başarıyla oluşturuldu."));
    }

    // POST /api/auth/login
    // [AllowAnonymous]: Token olmadan herkes erişebilir.
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var result = await _authService.LoginAsync(dto.Username, dto.Password);

        if (result is null)
            return Unauthorized(ApiResponse.Fail("Kullanıcı adı veya şifre hatalı."));

        return Ok(ApiResponse<LoginResponseDto>.Ok(result));
    }

    // POST /api/auth/refresh
    // [AllowAnonymous]: Süresi dolmuş access token yerine refresh token gönderilir.
    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequestDto dto)
    {
        var result = await _authService.RefreshTokenAsync(dto.RefreshToken);

        if (result is null)
            return Unauthorized(ApiResponse.Fail("Refresh token geçersiz veya süresi dolmuş. Lütfen tekrar giriş yapınız."));

        return Ok(ApiResponse<LoginResponseDto>.Ok(result));
    }

    // POST /api/auth/logout
    // FallbackPolicy → geçerli token gereklidir.
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (userIdClaim is null || !int.TryParse(userIdClaim, out var userId))
            return Unauthorized(ApiResponse.Fail("Geçersiz token."));

        await _authService.LogoutAsync(userId);

        return Ok(ApiResponse.Ok("Çıkış yapıldı."));
    }

    // GET /api/auth/me
    // [Authorize(Policy = "AllowedUser")]: Hem Admin hem User erişebilir.
    // Token'daki claim'lerden kullanıcı bilgisini okur — DB sorgusu yapmaz.
    // Kullanım: Frontend token'ın hangi kullanıcıya ait olduğunu doğrulamak ister.
    [HttpGet("me")]
    [Authorize(Policy = "AllowedUser")]
    public IActionResult Me()
    {
        var userIdClaim  = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var username     = User.FindFirstValue(ClaimTypes.Name);
        var role         = User.FindFirstValue(ClaimTypes.Role);

        if (userIdClaim is null || !int.TryParse(userIdClaim, out var userId))
            return Unauthorized(ApiResponse.Fail("Geçersiz token."));

        var dto = new MeResponseDto(userId, username ?? string.Empty, role ?? string.Empty);
        return Ok(ApiResponse<MeResponseDto>.Ok(dto));
    }
}
