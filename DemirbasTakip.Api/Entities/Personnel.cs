namespace DemirbasTakip.Api.Entities;

// Personel tablosuna karşılık gelir. Her personele zimmet atanabilir.
public class Personnel
{
    public int Id { get; set; }

    // Ad Soyad
    public string FullName { get; set; } = string.Empty;

    // Çalıştığı departman, örn: "Bilgi İşlem", "İnsan Kaynakları"
    public string Department { get; set; } = string.Empty;

    // Bu personele ait tüm zimmet kayıtları (geçmiş + aktif).
    public ICollection<AssetAssignment> Assignments { get; set; } = new List<AssetAssignment>();
}