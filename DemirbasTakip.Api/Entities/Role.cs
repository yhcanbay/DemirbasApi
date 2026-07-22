// Java'da "package", C#'ta "namespace" denir.
// Bu dosyadaki sınıf artık DemirbasTakip.Api.Entities isim uzayına ait.
namespace DemirbasTakip.Api.Entities;

// "Entity" = veritabanındaki tabloya karşılık gelen sınıf.
// EF Core bu sınıfı okuyarak "Roles" tablosunu oluşturur.
public class Role
{
    // Primary key — EF Core "Id" adını görünce otomatik PK yapar (Spring Data gibi).
    public int Id { get; set; }

    // C# naming convention: property adları PascalCase (büyük harf ile başlar).
    // Java'da "getName()" yazardın; C#'ta getter/setter'ı { get; set; } ile tanımlarsın.
    public string Name { get; set; } = string.Empty;   // null yerine boş string ile başlatıyoruz

    // Navigation property: bir Role'ün birden fazla User'ı olabilir (1-N ilişki).
    // Bu, Spring'teki @OneToMany gibi düşünülebilir — EF Core join'i otomatik yapar.
    public ICollection<User> Users { get; set; } = new List<User>();
}