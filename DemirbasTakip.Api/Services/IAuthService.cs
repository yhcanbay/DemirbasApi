using DemirbasTakip.Api.DTOs;

// "Services" isim uzayı: iş mantığı (business logic) katmanı.
namespace DemirbasTakip.Api.Services;

// Interface = Spring'deki @Service arayüzü gibi soyutlama katmanı.
// Controller bu arayüzü kullanır, somut sınıfı (AuthService) bilmez — bağımlılık tersine çevrilir.
public interface IAuthService
{
    // Task<T> = Java'daki CompletableFuture<T> veya Mono<T> gibi asenkron dönüş tipi.
    // "?" işareti = dönüş değeri null olabilir (kullanıcı bulunamazsa veya şifre yanlışsa).
    Task<LoginResponseDto?> LoginAsync(string username, string password);

    // true = başarılı kayıt
    // null = kullanıcı adı zaten mevcut
    // false = geçersiz rol adı
    Task<bool?> RegisterAsync(string username, string password, string roleName);

    // Refresh token geçerli ve süresi dolmamışsa yeni access token + refresh token döner.
    // Geçersiz veya süresi dolmuşsa null döner (controller 401 üretir).
    // Refresh sırasında süresi geçmiş tüm token'lar DB'den temizlenir.
    Task<LoginResponseDto?> RefreshTokenAsync(string refreshToken);

    // Kullanıcıya ait tüm refresh token'ları siler (logout).
    // userId: token'dan parse edilmiş kullanıcı kimliği.
    Task LogoutAsync(int userId);
}