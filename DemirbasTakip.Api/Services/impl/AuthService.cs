using Microsoft.EntityFrameworkCore;  // Include(), FirstOrDefaultAsync() için
using DemirbasTakip.Api.Data;         // AppDbContext'e erişim için
using DemirbasTakip.Api.DTOs;         // LoginResponseDto için
using DemirbasTakip.Api.Auth;         // TokenService için

namespace DemirbasTakip.Api.Services;

// IAuthService interface'ini implement eden somut sınıf.
// Spring'teki @Service anotasyonlu sınıfa karşılık gelir.
public class AuthService : IAuthService
{
    // Veritabanına erişmek için DbContext (Spring'deki @Autowired UserRepository gibi).
    private readonly AppDbContext _context;

    // JWT token üretici servis
    private readonly TokenService _tokenService;

    // Constructor: DI sistemi bu parametreleri otomatik olarak doldurur.
    public AuthService(AppDbContext context, TokenService tokenService)
    {
        _context = context;
        _tokenService = tokenService;
    }

    // Kullanıcı adı ve şifreyi doğrulayıp token döndürür.
    // Başarısız olursa null döner (controller 401 Unauthorized üretir).
    public async Task<LoginResponseDto?> LoginAsync(string username, string password)
    {
        // Kullanıcıyı veritabanından çek.
        // Include(u => u.Role) = SQL'deki JOIN ile Role tablosunu da getirir (Spring'deki EAGER fetch gibi).
        // FirstOrDefaultAsync = kullanıcı yoksa null döner, istisnai durum fırlatmaz.
        var user = await _context.Users
            .Include(u => u.Role)                               // Role navigation property'sini de yükle
            .FirstOrDefaultAsync(u => u.Username == username);  // WHERE Username = @username

        // Kullanıcı yoksa null dön
        if (user is null) return null;

        // BCrypt.Verify: kullanıcının girdiği şifreyi, veritabanındaki hash ile karşılaştırır.
        // Düz şifreyi hiçbir zaman veritabanında veya bellekte tutmuyoruz.
        // Bu, Spring Security'deki BCryptPasswordEncoder.matches() ile aynı işlevi görür.
        if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash)) return null;

        // Doğrulama başarılı: JWT token üret ve cevabı döndür.
        var token = _tokenService.CreateToken(user);
        return new LoginResponseDto(token, user.Username, user.Role.Name);
    }

    // Başarılı olursa true, kullanıcı adı zaten varsa null, geçersiz rol adıysa false döner.
    public async Task<bool?> RegisterAsync(string username, string password, string roleName)
    {
        // Kullanıcı adı zaten var mı? AnyAsync = SQL'deki EXISTS gibi, çok performanslı.
        var existingUser = await _context.Users.AnyAsync(u => u.Username == username);
        if (existingUser)
            return null;  // null = "kullanıcı adı zaten alınmış"

        // Rol adı geçerli mi? ("Admin" veya "User" olmalı)
        var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == roleName);
        if (role is null)
            return false;  // false = "geçersiz rol adı"

        // Şifreyi hiçbir zaman düz metin saklamayız.
        // BCrypt.HashPassword = Spring Security'deki BCryptPasswordEncoder.encode() gibi.
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

        var user = new User
        {
            Username = username,
            PasswordHash = passwordHash,
            RoleId = role.Id,
            Role = role
        };

        _context.Users.Add(user);          // Kullanıcıyı EF Core'un takip listesine ekle (bellekte)
        await _context.SaveChangesAsync(); // Belleктeki değişikliği SQL INSERT'e dönüştür ve DB'ye yaz

        return true;  // Kayıt başarılı
    }
}