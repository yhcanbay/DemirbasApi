namespace DemirbasTakip.Api.DTOs;

// Yeni bir departman oluşturmak için istemciden alınan veri şablonu.
public record CreateDepartmentDto(
    string DepartmentName  // Örn: "Bilgi İşlem", "İnsan Kaynakları"
);
