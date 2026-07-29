namespace DemirbasTakip.Api.DTOs;

// Demirbaş bilgilerini istemciye döndürmek için kullanılan cevap şablonu.
// Entity'nin tamamını değil, sadece istemcinin ihtiyaç duyduğu alanları taşır.
// Navigation property (Assignments listesi) burada kasıtlı olarak yer almaz —
// zimmet geçmişi için ayrı bir endpoint kullanılır.
public record AssetResponseDto(
    int Id,
    string Code,
    string Name,
    string Category,
    string Status,
    string SerialNumber
);
