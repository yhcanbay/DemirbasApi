namespace DemirbasTakip.Api.DTOs;

// Zimmet kaydını istemciye döndürmek için kullanılan cevap şablonu.
// AssetId/PersonnelId yerine okunabilir Ad bilgileri eklendi —
// istemci ek sorgu yapmadan gösterebilir.
public record AssetAssignmentResponseDto(
    int Id,
    int AssetId,
    string AssetName,       // Demirbaşın adı (join ile gelir)
    string AssetCode,       // Demirbaşın envanter kodu
    int PersonnelId,
    string PersonnelName,   // Personelin adı soyadı (join ile gelir)
    DateTime AssignedDate,
    DateTime? ReturnedDate  // null = hâlâ zimmette
);
