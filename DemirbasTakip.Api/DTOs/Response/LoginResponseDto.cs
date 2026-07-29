namespace DemirbasTakip.Api.DTOs;

// Başarılı login sonrasında istemciye dönecek cevap şablonu.
// İçinde JWT token, kullanıcı adı ve rolü gönderilir.
// İstemci bu token'ı bir sonraki isteklerde "Authorization: Bearer <token>" header'ında gönderir.
public record LoginResponseDto(string Token, string Username, string Role);