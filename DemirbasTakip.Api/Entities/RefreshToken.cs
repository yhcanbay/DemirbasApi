namespace DemirbasTakip.Api.Entities;

// Kullanıcının refresh token kaydını tutan entity.
// Her login işleminde yeni bir kayıt oluşturulur.
// Logout'ta kullanıcıya ait tüm kayıtlar silinir.
// Token yenilenirken süresi geçmiş kayıtlar otomatik temizlenir.
public class RefreshToken
{
    public int Id { get; set; }

    // Rastgele üretilen güvenli token string'i — Guid tabanlı
    public string Token { get; set; } = string.Empty;

    // Foreign key: Bu token hangi kullanıcıya ait?
    public int UserId { get; set; }

    // Navigation property: UserId üzerinden ilgili User nesnesine erişim
    public User User { get; set; } = null!;

    // Token'ın geçerlilik bitiş tarihi (oluşturma anı + RefreshTokenExpireMinutes)
    public DateTime ExpiresAt { get; set; }

    // Token'ın oluşturulma zamanı (bilgi amaçlı)
    public DateTime CreatedAt { get; set; }
}
