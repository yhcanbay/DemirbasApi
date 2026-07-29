namespace DemirbasTakip.Api.DTOs;

// Mevcut bir departmanın adını güncellemek için istemciden alınan veri şablonu.
public record UpdateDepartmentDto(
    string DepartmentName
);
