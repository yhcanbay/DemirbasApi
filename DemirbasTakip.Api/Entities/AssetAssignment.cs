namespace DemirbasTakip.Api.Entities;

// Zimmet kaydı: hangi demirbaşın hangi personelde olduğunu tutar.
// Bu, Asset ile Personnel arasındaki ara (junction) tablo — Spring'teki @ManyToMany gibi ama açık entity olarak.
public class AssetAssignment
{
    public int Id { get; set; }

    // Hangi demirbaş zimmetlendiğinin foreign key'i
    public int AssetId { get; set; }

    // Navigation property: AssetId üzerinden ilgili Asset nesnesine erişim sağlar
    public Asset Asset { get; set; } = null!;

    // Hangi personele zimmetlendiğinin foreign key'i
    public int PersonnelId { get; set; }

    // Navigation property: PersonnelId üzerinden ilgili Personnel nesnesine erişim sağlar
    public Personnel Personnel { get; set; } = null!;

    // Zimmet başlangıç tarihi — DateTime.UtcNow ile doldurulacak
    public DateTime AssignedDate { get; set; }

    // İade tarihi. null = hâlâ zimmette demek.
    // "?" işareti C#'ta "bu alan null olabilir" anlamına gelir (Java'daki Optional gibi düşün).
    public DateTime? ReturnedDate { get; set; }
}