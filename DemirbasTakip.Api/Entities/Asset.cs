// Asset.cs zaten doğru namespace ile yazılmıştı, burada da tutarlı kalıyoruz.
namespace DemirbasTakip.Api.Entities;

// Demirbaş (zimmet edilebilir eşya) tablosuna karşılık gelir.
public class Asset
{
    public int Id { get; set; }

    // Envanter kodu, örn: "BLG-001"
    public string Code { get; set; } = string.Empty;

    // Demirbaşın adı, örn: "Dell Laptop"
    public string Name { get; set; } = string.Empty;

    // Kategori, örn: "Bilgisayar", "Mobilya"
    public string Category { get; set; } = string.Empty;

    // Durum: "Aktif", "Pasif", "Arızalı" gibi değerler alabilir.
    // Varsayılan olarak "Aktif" ile başlıyoruz.
    public string Status { get; set; } = "Aktif";

    // Seri numarası
    public string SerialNumber { get; set; } = string.Empty;

    // Bir demirbaşın birden fazla zimmet kaydı olabilir (geçmiş kayıtlar dahil).
    // Bu navigation property EF Core'a "Asset → AssetAssignment" 1-N ilişkisini anlatır.
    public ICollection<AssetAssignment> Assignments { get; set; } = new List<AssetAssignment>();
}