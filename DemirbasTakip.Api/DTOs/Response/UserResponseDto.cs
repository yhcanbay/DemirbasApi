namespace DemirbasTakip.Api.DTOs;

// Kullanıcı bilgilerini istemciye döndürmek için kullanılan cevap şablonu.
// PasswordHash asla istemciye gönderilmez.
public record UserResponseDto(
    int Id,
    string Username,
    string RoleName,
    // Bağlı Personnel kaydının bilgileri (Admin kullanıcısı için null olabilir)
    int? PersonnelId,
    string? FullName
);
