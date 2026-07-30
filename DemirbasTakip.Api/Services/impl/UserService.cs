using Microsoft.EntityFrameworkCore;
using DemirbasTakip.Api.Data;
using DemirbasTakip.Api.DTOs;
using DemirbasTakip.Api.DTOs.Request.Create;
using DemirbasTakip.Api.Entities;

namespace DemirbasTakip.Api.Services;

public class UserService : IUserService
{
    private readonly AppDbContext _context;

    public UserService(AppDbContext context) => _context = context;

    // Tüm kullanıcıları Role ve bağlı Personnel bilgileriyle birlikte getirir.
    // Personnel navigation property User'da olmadığı için LEFT JOIN'i Personnel tarafından kuruyoruz.
    public async Task<List<UserResponseDto>> GetAllAsync()
    {
        return await _context.Users
            .Include(u => u.Role)
            .Select(u => new UserResponseDto(
                u.Id,
                u.Username,
                u.Role.Name,
                // Personnel tablosunda bu User'a ait kayıt var mı? (LEFT JOIN benzeri)
                _context.Personnel.Where(p => p.UserId == u.Id).Select(p => (int?)p.Id).FirstOrDefault(),
                _context.Personnel.Where(p => p.UserId == u.Id).Select(p => p.FullName).FirstOrDefault()
            ))
            .ToListAsync();
    }

    // Tek kullanıcıyı Id ile getirir.
    public async Task<UserResponseDto?> GetByIdAsync(int id)
    {
        var user = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user is null) return null;

        // Personnel tablosunda bu kullanıcıya ait kayıt ara
        var personnel = await _context.Personnel.FirstOrDefaultAsync(p => p.UserId == id);

        return new UserResponseDto(
            user.Id,
            user.Username,
            user.Role.Name,
            personnel?.Id,
            personnel?.FullName
        );
    }

    // Yeni User + Personnel kaydını tek bir transaction içinde oluşturur.
    // null  = kullanıcı adı zaten mevcut
    // false = geçersiz rol Id'si
    // true  = başarılı
    public async Task<bool?> CreateAsync(CreateUserDto dto)
    {
        // Kullanıcı adı benzersizlik kontrolü
        var usernameExists = await _context.Users.AnyAsync(u => u.Username == dto.Username);
        if (usernameExists) return null;

        // Rol geçerlilik kontrolü
        var role = await _context.Roles.FirstOrDefaultAsync(r => r.Id == dto.RoleId);
        if (role is null) return false;

        // Şifreyi hash'le — düz metin asla saklanmaz.
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

        var user = new User
        {
            Username     = dto.Username,
            PasswordHash = passwordHash,
            RoleId       = role.Id
        };

        _context.Users.Add(user);

        // Admin kullanıcısı için Personnel oluşturulmaz (RoleId = 1).
        // User rolü için (RoleId = 2) Personnel kaydı da oluşturulur.
        if (role.Id != 1)
        {
            var personnel = new Personnel
            {
                FullName = dto.FullName,
                User     = user   // EF Core SaveChanges sırasında UserId'yi otomatik bağlar
            };
            _context.Personnel.Add(personnel);
        }

        await _context.SaveChangesAsync();
        return true;
    }

    // Kullanıcıyı ve varsa bağlı Personnel kaydını siler.
    // false = kullanıcı bulunamadı | true = silme başarılı
    public async Task<bool> DeleteAsync(int id)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (user is null) return false;

        // Personnel varsa önce onu sil (FK kısıtı: Restrict olduğu için önce bağımlı silinmeli)
        var personnel = await _context.Personnel.FirstOrDefaultAsync(p => p.UserId == id);
        if (personnel is not null)
            _context.Personnel.Remove(personnel);

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
        return true;
    }
}
