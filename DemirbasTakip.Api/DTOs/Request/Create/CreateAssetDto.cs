namespace DemirbasTakip.Api.DTOs;

// Yeni bir demirbaş oluşturmak için istemciden alınan veri şablonu.
// Id, Status gibi alanlar istemci tarafından gönderilmez — sunucu belirler.
public record CreateAssetDto(
    string Code,         // Envanter kodu, örn: "BLG-001"
    string Name,         // Demirbaş adı, örn: "Dell Laptop"
    string Category,     // Kategori, örn: "Bilgisayar"
    string SerialNumber  // Seri numarası
);
