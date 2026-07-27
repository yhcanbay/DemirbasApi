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
public class AuthController : ControllerBase,IAuthController
{
    // Servise interface üzerinden erişiyoruz — somut sınıfı (AuthService) bilmiyoruz.
    private readonly IAuthService _authService;

    // DI sistemi IAuthService'i otomatik olarak AuthService örneğiyle doldurur.
    public AuthController(IAuthService authService) => _authService = authService;

    // POST /api/auth/login
    // [HttpPost("login")] = Spring'deki @PostMapping("/login") ile aynı.
    // [FromBody] = request body'deki JSON'ı LoginDto nesnesine dönüştür.
    //   Spring'deki @RequestBody LoginDto dto ile aynı.
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        // Servisi çağır: kullanıcı adı ve şifre doğrulansın, token üretilsin.
        var result = await _authService.LoginAsync(dto.Username, dto.Password);

        // Sonuç null ise kullanıcı adı/şifre yanlış — 401 döndür.
        if (result is null)
            return Unauthorized(new { message = "Kullanıcı adı veya şifre hatalı." });

        // Başarılı: 200 OK + token, kullanıcı adı ve rol bilgisiyle cevap dön.
        return Ok(result);
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
