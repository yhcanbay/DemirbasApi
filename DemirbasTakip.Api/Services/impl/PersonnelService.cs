using Microsoft.EntityFrameworkCore;
using DemirbasTakip.Api.Data;
using DemirbasTakip.Api.DTOs;
using DemirbasTakip.Api.Entities;

namespace DemirbasTakip.Api.Services;

public class PersonnelService : IPersonnelService
{
    private readonly AppDbContext _context;

    public PersonnelService(AppDbContext context) => _context = context;

    // Tüm personeli getirir. Aktif departman: EndDate == null olan PersonnelDepartment.
    public async Task<List<PersonnelResponseDto>> GetAllAsync()
    {
        return await _context.Personnel
            .Select(p => new PersonnelResponseDto(
                p.Id,
                p.FullName,
                // Aktif departman (EndDate == null) varsa adını döndür, yoksa null
                p.PersonnelDepartments
                    .Where(pd => pd.EndDate == null)
                    .Select(pd => pd.Department.DepartmentName)
                    .FirstOrDefault()
            ))
            .ToListAsync();
    }

    // Tek personeli Id ile getirir.
    public async Task<PersonnelResponseDto?> GetByIdAsync(int id)
    {
        var personnel = await _context.Personnel
            .Include(p => p.PersonnelDepartments)
                .ThenInclude(pd => pd.Department)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (personnel is null) return null;

        var activeDepartment = personnel.PersonnelDepartments
            .FirstOrDefault(pd => pd.EndDate == null)
            ?.Department.DepartmentName;

        return new PersonnelResponseDto(personnel.Id, personnel.FullName, activeDepartment);
    }

    // Personel adını günceller.
    // false = bulunamadı | true = başarılı
    public async Task<bool> UpdateAsync(int id, UpdatePersonnelDto dto)
    {
        var personnel = await _context.Personnel.FirstOrDefaultAsync(p => p.Id == id);
        if (personnel is null) return false;

        personnel.FullName = dto.FullName;
        await _context.SaveChangesAsync();
        return true;
    }

    // Personeli siler.
    // null  = aktif zimmeti var (Conflict)
    // false = bulunamadı
    // true  = başarılı
    public async Task<bool?> DeleteAsync(int id)
    {
        var personnel = await _context.Personnel
            .Include(p => p.Assignments)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (personnel is null) return false;

        // Aktif zimmet kontrolü: ReturnedDate == null olanlar hâlâ zimmette.
        var hasActiveAssignment = personnel.Assignments.Any(a => a.ReturnedDate == null);
        if (hasActiveAssignment) return null;

        _context.Personnel.Remove(personnel);
        await _context.SaveChangesAsync();
        return true;
    }
}