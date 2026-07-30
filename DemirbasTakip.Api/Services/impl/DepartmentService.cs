using Microsoft.EntityFrameworkCore;
using DemirbasTakip.Api.Data;
using DemirbasTakip.Api.DTOs;
using DemirbasTakip.Api.Entities;

namespace DemirbasTakip.Api.Services;

public class DepartmentService : IDepartmentService
{
    private readonly AppDbContext _context;

    public DepartmentService(AppDbContext context) => _context = context;

    // Tüm departmanları, her birinin aktif personel sayısıyla birlikte getirir.
    public async Task<List<DepartmentResponseDto>> GetAllAsync()
    {
        return await _context.Departments
            .Select(d => new DepartmentResponseDto(
                d.Id,
                d.DepartmentName,
                // Aktif personel: EndDate == null olan PersonnelDepartment sayısı
                d.PersonnelDepartments.Count(pd => pd.EndDate == null)
            ))
            .ToListAsync();
    }

    // Tek departmanı Id ile getirir.
    public async Task<DepartmentResponseDto?> GetByIdAsync(int id)
    {
        var dept = await _context.Departments
            .Include(d => d.PersonnelDepartments)
            .FirstOrDefaultAsync(d => d.Id == id);

        if (dept is null) return null;

        var activeCount = dept.PersonnelDepartments.Count(pd => pd.EndDate == null);
        return new DepartmentResponseDto(dept.Id, dept.DepartmentName, activeCount);
    }

    // Yeni departman oluşturur ve yeni Id'yi döndürür.
    public async Task<int> CreateAsync(CreateDepartmentDto dto)
    {
        var dept = new Department { DepartmentName = dto.DepartmentName };
        _context.Departments.Add(dept);
        await _context.SaveChangesAsync();
        return dept.Id;
    }

    // Departman adını günceller.
    // false = bulunamadı | true = başarılı
    public async Task<bool> UpdateAsync(int id, UpdateDepartmentDto dto)
    {
        var dept = await _context.Departments.FirstOrDefaultAsync(d => d.Id == id);
        if (dept is null) return false;

        dept.DepartmentName = dto.DepartmentName;
        await _context.SaveChangesAsync();
        return true;
    }

    // Departmanı siler.
    // null  = aktif personeli var (Conflict)
    // false = bulunamadı
    // true  = başarılı
    public async Task<bool?> DeleteAsync(int id)
    {
        var dept = await _context.Departments
            .Include(d => d.PersonnelDepartments)
            .FirstOrDefaultAsync(d => d.Id == id);

        if (dept is null) return false;

        // Aktif personeli varsa silmeyi engelle
        var hasActivePersonnel = dept.PersonnelDepartments.Any(pd => pd.EndDate == null);
        if (hasActivePersonnel) return null;

        _context.Departments.Remove(dept);
        await _context.SaveChangesAsync();
        return true;
    }

    // Personeli departmana atar; PersonnelDepartment kaydı oluşturur.
    // null  = personel veya departman bulunamadı
    // false = personel zaten bu departmanda aktif kayıtlı
    // true  = başarılı
    public async Task<bool?> AssignPersonnelAsync(AssignPersonnelToDepartmentDto dto)
    {
        var personnelExists = await _context.Personnel.AnyAsync(p => p.Id == dto.PersonnelId);
        var deptExists = await _context.Departments.AnyAsync(d => d.Id == dto.DepartmentId);

        if (!personnelExists || !deptExists) return null;

        // Aynı departmanda zaten aktif kayıt var mı?
        var alreadyActive = await _context.PersonnelDepartments
            .AnyAsync(pd =>
                pd.PersonnelId == dto.PersonnelId &&
                pd.DepartmentId == dto.DepartmentId &&
                pd.EndDate == null);

        if (alreadyActive) return false;

        var pd = new PersonnelDepartment
        {
            PersonnelId  = dto.PersonnelId,
            DepartmentId = dto.DepartmentId,
            StartDate    = dto.StartDate
        };

        _context.PersonnelDepartments.Add(pd);
        await _context.SaveChangesAsync();
        return true;
    }

    // Personeli departmandan çıkarır (EndDate = UtcNow).
    // false = aktif atama bulunamadı | true = başarılı
    public async Task<bool> RemovePersonnelAsync(int departmentId, int personnelId)
    {
        var pd = await _context.PersonnelDepartments
            .FirstOrDefaultAsync(pd =>
                pd.DepartmentId == departmentId &&
                pd.PersonnelId == personnelId &&
                pd.EndDate == null);

        if (pd is null) return false;

        pd.EndDate = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }
}
