using Microsoft.EntityFrameworkCore;
using DemirbasTakip.Api.Data;
using DemirbasTakip.Api.DTOs;
using DemirbasTakip.Api.Entities;

namespace DemirbasTakip.Api.Services;

public class AssetService : IAssetService
{
    private readonly AppDbContext _context;

    public AssetService(AppDbContext context) => _context = context;

    // Tüm demirbaşları listeler.
    public async Task<List<AssetResponseDto>> GetAllAsync()
    {
        return await _context.Assets
            .Select(a => new AssetResponseDto(
                a.Id, a.Code, a.Name, a.Category, a.Status, a.SerialNumber
            ))
            .ToListAsync();
    }

    // Tek demirbaşı Id ile getirir.
    public async Task<AssetResponseDto?> GetByIdAsync(int id)
    {
        var asset = await _context.Assets.FirstOrDefaultAsync(a => a.Id == id);
        if (asset is null) return null;

        return new AssetResponseDto(
            asset.Id, asset.Code, asset.Name, asset.Category, asset.Status, asset.SerialNumber
        );
    }

    // Yeni demirbaş oluşturur.
    // null  = Code zaten kullanımda (Conflict)
    // true  = başarılı
    public async Task<bool?> CreateAsync(CreateAssetDto dto)
    {
        // Envanter kodunun benzersizliğini kontrol et
        var codeExists = await _context.Assets.AnyAsync(a => a.Code == dto.Code);
        if (codeExists) return null;

        var asset = new Asset
        {
            Code         = dto.Code,
            Name         = dto.Name,
            Category     = dto.Category,
            SerialNumber = dto.SerialNumber,
            Status       = "Aktif"  // Yeni demirbaş varsayılan olarak aktif
        };

        _context.Assets.Add(asset);
        await _context.SaveChangesAsync();
        return true;
    }

    // Demirbaş bilgilerini günceller (Status dahil).
    // false = bulunamadı | true = başarılı
    public async Task<bool> UpdateAsync(int id, UpdateAssetDto dto)
    {
        var asset = await _context.Assets.FirstOrDefaultAsync(a => a.Id == id);
        if (asset is null) return false;

        asset.Code         = dto.Code;
        asset.Name         = dto.Name;
        asset.Category     = dto.Category;
        asset.SerialNumber = dto.SerialNumber;
        asset.Status       = dto.Status;

        await _context.SaveChangesAsync();
        return true;
    }

    // Demirbaşı siler.
    // null  = aktif zimmeti var (ReturnedDate == null)
    // false = bulunamadı
    // true  = başarılı
    public async Task<bool?> DeleteAsync(int id)
    {
        var asset = await _context.Assets
            .Include(a => a.Assignments)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (asset is null) return false;

        // Aktif zimmet kontrolü
        var hasActiveAssignment = asset.Assignments.Any(a => a.ReturnedDate == null);
        if (hasActiveAssignment) return null;

        _context.Assets.Remove(asset);
        await _context.SaveChangesAsync();
        return true;
    }
}
