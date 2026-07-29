namespace DemirbasTakip.Api.DTOs;

// Bir personeli bir departmana atamak için istemciden alınan veri şablonu.
// PersonnelDepartment (ara tablo) kaydı oluşturur.
public record AssignPersonnelToDepartmentDto(
    int PersonnelId,    // Atanacak personelin Id'si
    int DepartmentId,   // Atanacağı departmanın Id'si
    DateTime StartDate  // Göreve başlama tarihi
);
