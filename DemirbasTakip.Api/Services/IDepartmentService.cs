using DemirbasTakip.Api.DTOs;

namespace DemirbasTakip.Api.Services;

public interface IDepartmentService
{
    // Tüm departmanları aktif personel sayısıyla listele
    Task<List<DepartmentResponseDto>> GetAllAsync();

    // Tek departmanı Id ile getir
    Task<DepartmentResponseDto?> GetByIdAsync(int id);

    // Yeni departman oluştur; yeni kaydın Id'sini döndür
    Task<int> CreateAsync(CreateDepartmentDto dto);

    // Departman adını güncelle
    // false = bulunamadı | true = başarılı
    Task<bool> UpdateAsync(int id, UpdateDepartmentDto dto);

    // Departmanı sil
    // null  = aktif personeli var (Conflict)
    // false = bulunamadı
    // true  = başarılı
    Task<bool?> DeleteAsync(int id);

    // Personeli departmana ata (PersonnelDepartment kaydı oluştur)
    // null  = personel veya departman bulunamadı
    // false = personel zaten bu departmanda aktif
    // true  = başarılı
    Task<bool?> AssignPersonnelAsync(AssignPersonnelToDepartmentDto dto);

    // Personeli departmandan çıkar (EndDate doldurur)
    // false = aktif atama bulunamadı | true = başarılı
    Task<bool> RemovePersonnelAsync(int departmentId, int personnelId);
}
