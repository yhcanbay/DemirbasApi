namespace DemirbasTakip.Api.DTOs;

// Mevcut bir personelin bilgilerini güncellemek için istemciden alınan veri şablonu.
public record UpdatePersonnelDto(
    string FullName
);
