namespace DemirbasTakip.Api.DTOs;

// Personel-Departman ilişki kaydını istemciye döndürmek için kullanılan cevap şablonu.
// Personelin geçmiş ve aktif departman atamalarını listelemek için kullanılır.
public record PersonnelDepartmentResponseDto(
    int Id,
    int PersonnelId,
    string PersonnelName,
    int DepartmentId,
    string DepartmentName,
    DateTime StartDate,
    DateTime? EndDate  // null = hâlâ bu departmanda aktif
);
