using DemirbasTakip.Api.DTOs;
using DemirbasTakip.Api.DTOs.Request.Create;

namespace DemirbasTakip.Api.Services;

public interface IUserService
{
    // Tüm kullanıcıları listele (Admin paneli için)
    Task<List<UserResponseDto>> GetAllAsync();

    // Tek kullanıcı getir
    Task<UserResponseDto?> GetByIdAsync(int id);

    // Yeni kullanıcı + personel kaydı oluştur (sadece Admin çağırır)
    // Dönüş: null = kullanıcı adı zaten var | false = geçersiz rol | true = başarılı
    Task<bool?> CreateAsync(CreateUserDto dto);

    // Kullanıcı sil (bağlı Personnel kaydını da siler)
    // Dönüş: false = bulunamadı | true = başarılı
    Task<bool> DeleteAsync(int id);
}
