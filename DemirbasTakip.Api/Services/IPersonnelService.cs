using DemirbasTakip.Api.DTOs;

namespace DemirbasTakip.Api.Services;

public interface IPersonnelService
{
    // Tüm personeli aktif departman bilgisiyle listele
    Task<List<PersonnelResponseDto>> GetAllAsync();

    // Tek personeli Id ile getir
    Task<PersonnelResponseDto?> GetByIdAsync(int id);

    // Personel adını güncelle
    // false = bulunamadı | true = başarılı
    Task<bool> UpdateAsync(int id, UpdatePersonnelDto dto);

    // Personeli sil — aktif zimmeti varsa işlem reddedilir
    // null  = aktif zimmeti var (Conflict)
    // false = bulunamadı
    // true  = başarılı
    Task<bool?> DeleteAsync(int id);
}