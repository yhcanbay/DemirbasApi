namespace DemirbasTakip.Api.Entities;

// Kullanıcı tablosuna karşılık gelen entity.
public class User
{
    public int Id { get; set; }

    public string Username { get; set; } = string.Empty;

    // Şifreyi asla düz metin saklamayız.
    // BCrypt ile hash'lenmiş hâli burada tutulur.
    public string PasswordHash { get; set; } = string.Empty;

    // Foreign key: hangi role ait olduğunu tutan sayısal sütun.
    // EF Core bunu Roles tablosundaki Id ile eşleştirir.
    public int RoleId { get; set; }

    // Navigation property: User nesnesine .Role deyince ilgili Role nesnesini getirir.
    // null! = "bu asla null olmayacak, ama derleyiciye güven" anlamında (C# nullable uyarısını susturur).
    public Role Role { get; set; } = null!;
}