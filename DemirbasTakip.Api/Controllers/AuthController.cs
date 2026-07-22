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
public class AuthController : ControllerBase
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

    // GET /api/auth/me
    // [Authorize]: Bu endpoint'e erişmek için geçerli JWT token zorunlu.
    //   Spring'deki @PreAuthorize("isAuthenticated()") ile aynı işlev.
    [HttpGet("me")]
    [Authorize]
    public IActionResult Me()
    {
        // Token doğrulandıktan sonra, token içindeki claim'lere User property'sinden erişilir.
        // Spring'de SecurityContextHolder.getContext().getAuthentication() ile alırdın.
        var username = User.Identity?.Name;
        var role = User.FindFirst(ClaimTypes.Role)?.Value;

        // Anonim nesne (new { ... }) JSON olarak serialize edilip döndürülür.
        return Ok(new { username, role });
    }
}
