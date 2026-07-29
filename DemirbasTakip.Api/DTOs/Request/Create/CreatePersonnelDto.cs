namespace DemirbasTakip.Api.DTOs;

// Yeni bir personel kaydı oluşturmak için istemciden alınan veri şablonu.
public record CreatePersonnelDto(
    string FullName  // Personelin ad ve soyadı
);
