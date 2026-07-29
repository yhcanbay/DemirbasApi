using Microsoft.EntityFrameworkCore;  // Include(), FirstOrDefaultAsync() için
using DemirbasTakip.Api.Data;         // AppDbContext'e erişim için
using DemirbasTakip.Api.DTOs;         // LoginResponseDto için
using DemirbasTakip.Api.Auth;         // TokenService için
using DemirbasTakip.Api.Entities;     // User, Role, RefreshToken sınıfları için

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

    // Kullanıcı adı ve şifreyi doğrulayıp access token + refresh token döndürür.
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

        // Doğrulama başarılı: access token üret.
        var accessToken = _tokenService.CreateToken(user);

        // Refresh token üret ve DB'ye kaydet.
        var refreshToken = _tokenService.CreateRefreshToken(user.Id);
        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync();

        return new LoginResponseDto(accessToken, user.Username, user.Role.Name, refreshToken.Token);
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

    // Refresh token geçerliyse yeni access token + refresh token döndürür.
    // Refresh Token Rotation: eski token silinir, yerine yeni token üretilir.
    // Süresi dolmuş tüm token'lar bu sırada DB'den temizlenir.
    public async Task<LoginResponseDto?> RefreshTokenAsync(string refreshToken)
    {
        // Önce: süresi geçmiş token'ları DB'den toplu temizle (bakım görevi).
        // Bu sayede tablo şişmez ve performans korunur.
        var now = DateTime.UtcNow;
        var expiredTokens = await _context.RefreshTokens
            .Where(rt => rt.ExpiresAt <= now)
            .ToListAsync();
        if (expiredTokens.Any())
        {
            _context.RefreshTokens.RemoveRange(expiredTokens);
            // SaveChanges aşağıda yapılacak, burada ayrıca çağırmıyoruz.
        }

        // Gelen refresh token'ı DB'de ara.
        // Include ile User ve User.Role'ü de yükle — yeni access token için gerekli.
        // NOT: ExpiresAt > now filtresi kritik — RemoveRange henüz DB'ye yazılmadığı için
        // süresi geçmiş tokenlar ikinci sorguda DB'den hâlâ gelebilirdi.
        var storedToken = await _context.RefreshTokens
            .Include(rt => rt.User)
            .ThenInclude(u => u.Role)   // User'ın Role'ünü de yükle
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken && rt.ExpiresAt > now);

        // Token DB'de yoksa (geçersiz, silinmiş veya hiç var olmamış) → 401
        if (storedToken is null)
        {
            // Temizlenen süresi dolmuşları kaydet ve çık
            if (expiredTokens.Any()) await _context.SaveChangesAsync();
            return null;
        }

        // Refresh Token Rotation:
        // Eski token'ı sil, yerine yeni bir token üret ve kaydet.
        // Bu, çalınmış bir token'ın yalnızca bir kez kullanılabilmesini garanti eder.
        var user = storedToken.User;
        _context.RefreshTokens.Remove(storedToken);

        var newRefreshToken = _tokenService.CreateRefreshToken(user.Id);
        _context.RefreshTokens.Add(newRefreshToken);

        await _context.SaveChangesAsync();

        // Yeni access token üret ve döndür.
        var newAccessToken = _tokenService.CreateToken(user);
        return new LoginResponseDto(newAccessToken, user.Username, user.Role.Name, newRefreshToken.Token);
    }

    // Kullanıcıya ait tüm refresh token'ları siler (logout).
    // Kullanıcı hangi cihazdan bağlı olursa olsun tüm oturumlar sonlandırılır.
    public async Task LogoutAsync(int userId)
    {
        // WHERE UserId = @userId olan tüm kayıtları getir ve sil.
        var userTokens = await _context.RefreshTokens
            .Where(rt => rt.UserId == userId)
            .ToListAsync();

        if (userTokens.Any())
        {
            _context.RefreshTokens.RemoveRange(userTokens);
            await _context.SaveChangesAsync();
        }
    }
}