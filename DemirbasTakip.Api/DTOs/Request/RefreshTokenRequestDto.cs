namespace DemirbasTakip.Api.DTOs;

// POST /api/auth/refresh endpoint'ine gönderilecek istek şablonu.
// İstemci, süresi dolan access token yerine yenisini almak için
// bu DTO'yu body'de gönderir.
public record RefreshTokenRequestDto(string RefreshToken);
