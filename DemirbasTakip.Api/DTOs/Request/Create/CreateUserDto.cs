namespace DemirbasTakip.Api.DTOs.Request.Create;

// Admin'in yeni çalışan eklerken kullandığı DTO.
// Tek bir istek ile hem User (login hesabı) hem Personnel (personel kaydı) oluşturulur.
public class CreateUserDto
{
    // --- Kullanıcı Hesabı Bilgileri ---

    // Giriş yaparken kullanılacak benzersiz kullanıcı adı.
    public string Username { get; set; } = string.Empty;

    // Admin tarafından belirlenen başlangıç şifresi.
    // Service katmanında BCrypt ile hash'lenerek saklanır; düz metin asla tutulmaz.
    public string Password { get; set; } = string.Empty;

    // Kullanıcının rolü: 1 = Admin, 2 = User
    // Seed data ile oluşturulan Role tablosundaki Id değerine karşılık gelir.
    public int RoleId { get; set; }

    // --- Personel Bilgileri ---

    // Ad Soyad — Personnel tablosuna yazılacak.
    // RoleId = 1 (Admin) olsa bile bu alan doldurulabilir; ancak Admin için Personnel oluşturulmaz.
    public string FullName { get; set; } = string.Empty;
}
