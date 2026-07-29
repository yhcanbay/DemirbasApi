namespace DemirbasTakip.Api.DTOs;

// Personel bilgilerini istemciye döndürmek için kullanılan cevap şablonu.
// Aktif departman bilgisi de eklendi — PersonnelDepartments.Where(pd => pd.EndDate == null)
// ile serviste hesaplanır.
public record PersonnelResponseDto(
    int Id,
    string FullName,
    string? ActiveDepartment  // null = henüz departman atanmamış
);
