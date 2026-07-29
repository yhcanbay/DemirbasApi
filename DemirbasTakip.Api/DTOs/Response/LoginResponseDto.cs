namespace DemirbasTakip.Api.DTOs;

// Başarılı login ve token yenileme sonrasında istemciye dönecek cevap şablonu.
// Token: kısa ömürlü access token (5 dk) — korumalı endpoint'lerde Authorization header'da kullanılır.
// RefreshToken: uzun ömürlü token (60 dk) — access token süresi dolunca yenileme için gönderilir.
public record LoginResponseDto(string Token, string Username, string Role, string RefreshToken);