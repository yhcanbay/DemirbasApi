namespace DemirbasTakip.Api.DTOs;

// Departman bilgilerini istemciye döndürmek için kullanılan cevap şablonu.
// PersonnelCount: bu departmanda şu an aktif kaç personel olduğunu gösterir.
public record DepartmentResponseDto(
    int Id,
    string DepartmentName,
    int PersonnelCount  // EndDate == null olan PersonnelDepartment kayıtlarının sayısı
);
