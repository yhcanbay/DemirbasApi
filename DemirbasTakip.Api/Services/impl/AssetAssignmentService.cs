using Microsoft.EntityFrameworkCore;
using DemirbasTakip.Api.Data;
using DemirbasTakip.Api.DTOs;
using DemirbasTakip.Api.Entities;

namespace DemirbasTakip.Api.Services;

public class AssetAssignmentService : IAssetAssignmentService
{
    private readonly AppDbContext _context;

    public AssetAssignmentService(AppDbContext context) => _context = context;

    // Tüm zimmet kayıtlarını demirbaş ve personel bilgileriyle birlikte getirir.
    public async Task<List<AssetAssignmentResponseDto>> GetAllAsync()
    {
        return await _context.AssetAssignments
            .Include(a => a.Asset)
            .Include(a => a.Personnel)
            .Select(a => new AssetAssignmentResponseDto(
                a.Id,
                a.AssetId,
                a.Asset.Name,
                a.Asset.Code,
                a.PersonnelId,
                a.Personnel.FullName,
                a.AssignedDate,
                a.ReturnedDate
            ))
            .ToListAsync();
    }

    // Belirli bir personelin tüm zimmet kayıtlarını getirir.
    public async Task<List<AssetAssignmentResponseDto>> GetByPersonnelIdAsync(int personnelId)
    {
        return await _context.AssetAssignments
            .Include(a => a.Asset)
            .Include(a => a.Personnel)
            .Where(a => a.PersonnelId == personnelId)
            .Select(a => new AssetAssignmentResponseDto(
                a.Id,
                a.AssetId,
                a.Asset.Name,
                a.Asset.Code,
                a.PersonnelId,
                a.Personnel.FullName,
                a.AssignedDate,
                a.ReturnedDate
            ))
            .ToListAsync();
    }

    // Belirli bir demirbaşın tüm zimmet geçmişini getirir.
    public async Task<List<AssetAssignmentResponseDto>> GetByAssetIdAsync(int assetId)
    {
        return await _context.AssetAssignments
            .Include(a => a.Asset)
            .Include(a => a.Personnel)
            .Where(a => a.AssetId == assetId)
            .Select(a => new AssetAssignmentResponseDto(
                a.Id,
                a.AssetId,
                a.Asset.Name,
                a.Asset.Code,
                a.PersonnelId,
                a.Personnel.FullName,
                a.AssignedDate,
                a.ReturnedDate
            ))
            .ToListAsync();
    }

    // Giriş yapan kullanıcının (User rolü) kendi zimmetlerini getirir.
    // userId → Personnel kaydı bulunur → o personelin zimmetleri listelenir.
    // null = bu userId'ye ait Personnel kaydı yok (Admin gibi)
    public async Task<List<AssetAssignmentResponseDto>?> GetMyAssignmentsAsync(int userId)
    {
        // UserId'den Personnel kaydını bul
        var personnel = await _context.Personnel
            .FirstOrDefaultAsync(p => p.UserId == userId);

        if (personnel is null) return null;

        return await _context.AssetAssignments
            .Include(a => a.Asset)
            .Include(a => a.Personnel)
            .Where(a => a.PersonnelId == personnel.Id)
            .Select(a => new AssetAssignmentResponseDto(
                a.Id,
                a.AssetId,
                a.Asset.Name,
                a.Asset.Code,
                a.PersonnelId,
                a.Personnel.FullName,
                a.AssignedDate,
                a.ReturnedDate
            ))
            .ToListAsync();
    }

    // Demirbaşı personele zimmetler.
    // null  = demirbaş zaten aktif zimmette (Conflict)
    // false = demirbaş veya personel bulunamadı
    // true  = başarılı
    public async Task<bool?> AssignAsync(CreateAssetAssignmentDto dto)
    {
        var assetExists = await _context.Assets.AnyAsync(a => a.Id == dto.AssetId);
        var personnelExists = await _context.Personnel.AnyAsync(p => p.Id == dto.PersonnelId);

        if (!assetExists || !personnelExists) return false;

        // Demirbaş zaten aktif zimmette mi? (ReturnedDate == null)
        var alreadyAssigned = await _context.AssetAssignments
            .AnyAsync(a => a.AssetId == dto.AssetId && a.ReturnedDate == null);

        if (alreadyAssigned) return null;

        var assignment = new AssetAssignment
        {
            AssetId      = dto.AssetId,
            PersonnelId  = dto.PersonnelId,
            AssignedDate = DateTime.UtcNow
        };

        _context.AssetAssignments.Add(assignment);
        await _context.SaveChangesAsync();
        return true;
    }

    // Zimmeti iade eder (ReturnedDate = UtcNow).
    // false = zimmet bulunamadı veya zaten iade edilmiş
    // true  = başarılı
    public async Task<bool> ReturnAsync(int assignmentId)
    {
        var assignment = await _context.AssetAssignments
            .FirstOrDefaultAsync(a => a.Id == assignmentId && a.ReturnedDate == null);

        if (assignment is null) return false;

        assignment.ReturnedDate = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }
}
