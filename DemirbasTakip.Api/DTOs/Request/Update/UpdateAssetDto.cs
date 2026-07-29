namespace DemirbasTakip.Api.DTOs;

// Mevcut bir demirbaşı güncellemek için istemciden alınan veri şablonu.
// Status güncellenebilir (örn: "Aktif" → "Arızalı").
public record UpdateAssetDto(
    string Code,
    string Name,
    string Category,
    string SerialNumber,
    string Status  // "Aktif", "Pasif", "Arızalı" gibi değerler
);
