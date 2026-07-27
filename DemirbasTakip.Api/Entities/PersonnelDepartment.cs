namespace DemirbasTakip.Api.Entities;

// Personnel ile Department arasındaki M2M ilişkiyi tutan junction entity.
// Bir personelin farklı dönemlerde farklı departmanlarda çalışmasını modellemek için.
// Spring'teki explicit @Entity junction sınıfıyla aynı mantık.
public class PersonnelDepartment
{
    public int Id { get; set; }

    // --- Foreign Key'ler (açık tanımlama: performans + okunabilirlik) ---
    // EF Core bu int alanları otomatik FK olarak tanır (naming convention: {NavigationProperty}Id).
    public int PersonnelId { get; set; }
    public int DepartmentId { get; set; }

    // --- Navigation Property'ler ---
    // null! = "EF Core bunu yükleyecek, null olmayacak" garantisi — nullable uyarısını susturur.
    public Personnel Personnel { get; set; } = null!;
    public Department Department { get; set; } = null!;

    // Personelin bu departmanda çalışmaya başladığı tarih.
    public DateTime StartDate { get; set; }

    // Personelin bu departmandan ayrıldığı tarih.
    // null = hâlâ bu departmanda çalışıyor demek.
    // "Şu an hangi departmandalar?" → EndDate == null filtrelemesiyle bulunur.
    public DateTime? EndDate { get; set; }
}