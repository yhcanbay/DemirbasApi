namespace DemirbasTakip.Api.DTOs;

// Yeni bir kullanıcı oluşturmak için istemciden alınan veri şablonu.
// Şifre burada düz metin gelir; servis katmanında BCrypt ile hash'lenir.
// RoleId belirtilmezse varsayılan "User" (Id=2) rolü atanır.
public record RegisterDto(
    string Username,
    string Password,
    string RoleName = "User"  // Varsayılan: User rolü
);
