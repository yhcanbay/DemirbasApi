using DemirbasTakip.Api.DTOs;

namespace DemirbasTakip.Api.Services;

public interface IAssetAssignmentService
{
    // Tüm zimmet kayıtlarını listele (geçmiş dahil)
    Task<List<AssetAssignmentResponseDto>> GetAllAsync();

    // Belirli bir personelin tüm zimmet kayıtları
    Task<List<AssetAssignmentResponseDto>> GetByPersonnelIdAsync(int personnelId);

    // Belirli bir demirbaşın tüm zimmet geçmişi
    Task<List<AssetAssignmentResponseDto>> GetByAssetIdAsync(int assetId);

    // Giriş yapan kullanıcının (User rolü) kendi zimmetleri
    // userId → Personnel kaydı → Assignments akışıyla çalışır
    // null = bu userId'ye ait Personnel kaydı yok
    Task<List<AssetAssignmentResponseDto>?> GetMyAssignmentsAsync(int userId);

    // Demirbaşı personele zimmetle
    // null  = demirbaş zaten aktif zimmette
    // false = demirbaş veya personel bulunamadı
    // true  = başarılı
    Task<bool?> AssignAsync(CreateAssetAssignmentDto dto);

    // Zimmeti iade et (ReturnedDate = UtcNow)
    // false = zimmet bulunamadı veya zaten iade edilmiş
    // true  = başarılı
    Task<bool> ReturnAsync(int assignmentId);
}
