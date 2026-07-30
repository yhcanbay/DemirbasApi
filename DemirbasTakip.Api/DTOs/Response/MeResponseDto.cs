namespace DemirbasTakip.Api.DTOs;

// GET /api/auth/me endpoint'inin döndürdüğü cevap şablonu.
// Token'dan okunan bilgilerle doldurulur — DB sorgusu yapılmaz.
public record MeResponseDto(
    int UserId,
    string Username,
    string Role
);
