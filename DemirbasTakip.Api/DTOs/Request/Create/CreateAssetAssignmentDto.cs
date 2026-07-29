namespace DemirbasTakip.Api.DTOs;

// Bir demirbaşı personele zimmetlemek için istemciden alınan veri şablonu.
// AssignedDate sunucu tarafında otomatik atanır (DateTime.UtcNow).
public record CreateAssetAssignmentDto(
    int AssetId,      // Zimmetlenecek demirbaşın Id'si
    int PersonnelId   // Demirbaşı alacak personelin Id'si
);
