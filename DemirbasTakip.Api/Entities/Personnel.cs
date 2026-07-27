namespace DemirbasTakip.Api.Entities;

// Personel tablosuna karşılık gelir. Her personele zimmet atanabilir.
public class Personnel
{
    public int Id { get; set; }

    // Ad Soyad
    public string FullName { get; set; } = string.Empty;

    // Personelin çalıştığı departmanlar — M2M ilişki (geçmiş kayıtlar dahil).
    // Aktif departman: PersonnelDepartments.Where(pd => pd.EndDate == null)
    public ICollection<PersonnelDepartment> PersonnelDepartments { get; set; } = new List<PersonnelDepartment>();

    // Bu personele ait tüm zimmet kayıtları (geçmiş + aktif).
    public ICollection<AssetAssignment> Assignments { get; set; } = new List<AssetAssignment>();
}