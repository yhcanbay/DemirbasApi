using DemirbasTakip.Api.DTOs;

namespace DemirbasTakip.Api.Services;

public interface IAssetService
{
    // Tüm demirbaşları listele
    Task<List<AssetResponseDto>> GetAllAsync();

    // Tek demirbaşı Id ile getir
    Task<AssetResponseDto?> GetByIdAsync(int id);

    // Yeni demirbaş oluştur
    // null  = envanter kodu (Code) zaten kullanımda
    // true  = başarılı
    Task<bool?> CreateAsync(CreateAssetDto dto);

    // Demirbaş bilgilerini güncelle (Status dahil)
    // false = bulunamadı | true = başarılı
    Task<bool> UpdateAsync(int id, UpdateAssetDto dto);

    // Demirbaşı sil
    // null  = aktif zimmeti var (Conflict)
    // false = bulunamadı
    // true  = başarılı
    Task<bool?> DeleteAsync(int id);
}
