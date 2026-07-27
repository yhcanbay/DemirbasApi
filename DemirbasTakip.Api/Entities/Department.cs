namespace DemirbasTakip.Api.Entities;

// Departman tablosuna karşılık gelir.
// Personnel ile M2M ilişkisi vardır — PersonnelDepartment junction entity'si üzerinden.
public class Department
{
    public int Id { get; set; }

    // Departman adı, örn: "Bilgi İşlem", "İnsan Kaynakları"
    // C# naming convention: property adları PascalCase olmalı (Java'daki camelCase değil).
    public string DepartmentName { get; set; } = string.Empty;

    // Navigation property: Bu departmana ait tüm personel-departman kayıtları.
    // PersonnelDepartment üzerinden ilgili Personnel listesine erişilebilir.
    public ICollection<PersonnelDepartment> PersonnelDepartments { get; set; } = new List<PersonnelDepartment>();
}