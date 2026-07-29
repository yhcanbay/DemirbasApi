using Microsoft.AspNetCore.Authorization;  // [Authorize] attribute için
using Microsoft.AspNetCore.Mvc;            // ControllerBase, IActionResult, HttpPost vs. için
using System.Security.Claims;             // ClaimTypes için
using DemirbasTakip.Api.DTOs;             // LoginDto için
using DemirbasTakip.Api.Services;         // IAuthService için

// Controllers isim uzayı — tüm controller'lar burada.
namespace DemirbasTakip.Api.Controllers;

// [ApiController]: JSON serileştirme, model doğrulama gibi API özelliklerini otomatik açar.
//   Spring'deki @RestController gibi düşün.
// [Route("api/auth")]: Bu controller'ın tüm endpoint'leri "/api/auth/..." ile başlar.
//   Spring'deki @RequestMapping("/api/auth") gibi.
// ControllerBase: View (HTML sayfası) döndürmez; saf API için kullanılır.
//   Spring'deki @RestController (yani @Controller + @ResponseBody) ile aynı mantık.
[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase, IAuthController
{
    // Servise interface üzerinden erişiyoruz — somut sınıfı (AuthService) bilmiyoruz.
    private readonly IAuthService _authService;

    // DI sistemi IAuthService'i otomatik olarak AuthService örneğiyle doldurur.
    public AuthController(IAuthService authService) => _authService = authService;

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        var result = await _authService.RegisterAsync(dto.Username, dto.Password, dto.RoleName);

        // null = kullanıcı adı zaten mevcut
        if (result is null)
            return Conflict(new { message = "Bu kullanıcı adı zaten alınmış." });

        // false = geçersiz rol adı
        if (result == false)
            return BadRequest(new { message = "Geçersiz rol adı. 'Admin' veya 'User' olmalıdır." });

        // 201 Created: kayıt başarılı, istemci login sayfasına yönlendirilmeli.
        return StatusCode(201, new { message = "Kayıt başarılı. Lütfen giriş yapınız." });
    }

    // POST /api/auth/login
    // [HttpPost("login")] = Spring'deki @PostMapping("/login") ile aynı.
    // [FromBody] = request body'deki JSON'ı LoginDto nesnesine dönüştür.
    //   Spring'deki @RequestBody LoginDto dto ile aynı.
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        // Servisi çağır: kullanıcı adı ve şifre doğrulansın, token üretilsin.
        var result = await _authService.LoginAsync(dto.Username, dto.Password);

        // Sonuç null ise kullanıcı adı/şifre yanlış — 401 döndür.
        if (result is null)
            return Unauthorized(new { message = "Kullanıcı adı veya şifre hatalı." });

        // Başarılı: 200 OK + access token, kullanıcı adı, rol ve refresh token döndür.
        return Ok(result);
    }

    // POST /api/auth/refresh
    // Access token süresi dolduğunda istemci bu endpoint'e refresh token göndererek
    // yeni bir access token alır. Başarılı olursa yeni refresh token da döner (Rotation).
    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequestDto dto)
    {
        var result = await _authService.RefreshTokenAsync(dto.RefreshToken);

        // null = refresh token geçersiz veya süresi dolmuş → kullanıcı tekrar login olmalı
        if (result is null)
            return Unauthorized(new { message = "Refresh token geçersiz veya süresi dolmuş. Lütfen tekrar giriş yapınız." });

        return Ok(result);
    }

    // POST /api/auth/logout
    // JWT token'dan kullanıcı Id'sini okur, o kullanıcının tüm refresh token'larını siler.
    // [Authorize] gerekmez ama valid token beklenir (FallbackPolicy zaten zorunlu kılar).
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        // ClaimTypes.NameIdentifier = TokenService'te "new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())" ile set edildi.
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (userIdClaim is null || !int.TryParse(userIdClaim, out var userId))
            return Unauthorized(new { message = "Geçersiz token." });

        await _authService.LogoutAsync(userId);

        return Ok(new { message = "Çıkış yapıldı." });
    }

    // ============================================================
    // MİKROSERVİS NOTU: GET /api/auth/me
    // Bu endpoint şu an monolitik mimaride kullanılmamaktadır.
    // Monolitik yapıda her [Authorize] attribute'ü token'ı zaten otomatik
    // doğular; ayrıca bir /me endpoint'ine gerek yoktur.
    //
    // İleride mikroservis mimarisine geçilmesi durumunda bu endpoint devreye alınır:
    //   - Asset Servisi, Kargo Servisi gibi bağımsız servisler JWT secret key'e
    //     sahip olmaz; token'ı doğrulamak için Auth Servisi'ne (bu endpoint'e) sorar.
    //   - Token iptali (logout, hesap askıya alma) senaryolarında da merkezi
    //     doğrulama noktası olarak kullanılır.
    //
    // Aktif etmek için: aşağıdaki yorum satırlarını kaldır.
    // ============================================================

    //[HttpGet("me")]
    //[Authorize]
    //public IActionResult Me()
    //{
    //    var username = User.Identity?.Name;
    //    var role = User.FindFirst(ClaimTypes.Role)?.Value;
    //
    //    return Ok(new { username, role });
    //}
}
