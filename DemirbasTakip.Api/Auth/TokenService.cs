using System.IdentityModel.Tokens.Jwt;   // JWT token oluşturma sınıfları
using System.Security.Claims;            // Claim = token içine gömülen kullanıcı bilgisi birimi
using System.Text;                       // Encoding.UTF8 için
using Microsoft.IdentityModel.Tokens;   // SymmetricSecurityKey, SigningCredentials için
using DemirbasTakip.Api.Entities;       // User entity'sine erişim için

namespace DemirbasTakip.Api.Auth;

// JWT token'ı oluşturmakla sorumlu servis.
// Spring Security'deki JwtTokenProvider sınıfının karşılığı.
public class TokenService
{
    // IConfiguration = appsettings.json dosyasındaki değerlere erişim arayüzü.
    // Spring'teki @Value ile inject edilen @ConfigurationProperties gibi düşünülebilir.
    private readonly IConfiguration _config;

    // Constructor injection — DI (Dependency Injection) sistemi bu parametreyi otomatik doldurur.
    // "=>" expression body constructor: tek satırlık constructor için kısaltma.
    public TokenService(IConfiguration config) => _config = config;

    // User nesnesini alıp JWT token string'i döndürür.
    public string CreateToken(User user)
    {
        // Claim = token'ın içine gömülen bilgi parçası.
        // Token decode edildiğinde bu bilgiler okunabilir (şifrelenmez, sadece imzalanır!).
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()), // Kullanıcının ID'si
            new Claim(ClaimTypes.Name, user.Username),                 // Kullanıcı adı
            new Claim(ClaimTypes.Role, user.Role.Name)                 // Rolü (Admin/User)
        };

        // İmzalama anahtarı: appsettings.json'daki "Jwt:Key" değeri UTF-8 byte dizisine çevrilir.
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));

        // HmacSha256 = token'ı imzalamak için kullanılan algoritma.
        // Önemli: SecurityAlgorithms.HmacSha256 kullanılmalı, SecurityAlgorithms.Sha256 DEĞİL.
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // Token nesnesi oluşturuluyor.
        // issuer = token'ı kim üretti (backend API'miz)
        // audience = token kimin için üretildi (frontend istemcimiz)
        // expires = token ne zaman geçersiz olur
        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(double.Parse(_config["Jwt:ExpireMinutes"]!)),
            signingCredentials: creds);

        // JwtSecurityTokenHandler token nesnesini string'e (compact JWT formatına) çevirir.
        // Dönen değer "xxxxx.yyyyy.zzzzz" formatında base64url encoded bir string'dir.
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}