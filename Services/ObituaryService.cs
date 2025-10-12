using Microsoft.EntityFrameworkCore;
using ObituaryApp.Data;
using ObituaryApp.Models;

namespace ObituaryApp.Services;

public class ObituaryService : IObituaryService
{
    private readonly ApplicationDbContext _db;

    public ObituaryService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<Obituary> CreateAsync(Obituary obituary)
    {
        _db.Obituaries.Add(obituary);
        await _db.SaveChangesAsync();
        return obituary;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var e = await _db.Obituaries.FindAsync(id);
        if (e == null) return false;
        // remove physical file if present
        if (!string.IsNullOrEmpty(e.PhotoUrl) && e.PhotoUrl.StartsWith("/images/"))
        {
            var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", e.PhotoUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
            try
            {
                if (System.IO.File.Exists(oldPath)) System.IO.File.Delete(oldPath);
            }
            catch { /* ignore file delete errors */ }
        }
        _db.Obituaries.Remove(e);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<List<Obituary>> GetAllAsync(int page = 1, int pageSize = 20, string? search = null)
    {
        var query = _db.Obituaries.AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLowerInvariant();
            query = query.Where(o => o.FullName.ToLower().Contains(s));
        }

        return await query
            .OrderByDescending(o => o.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<Obituary?> GetByIdAsync(int id)
    {
        return await _db.Obituaries.FindAsync(id);
    }

    public async Task<bool> UpdateAsync(Obituary obituary)
    {
        var existing = await _db.Obituaries.FindAsync(obituary.Id);
        if (existing == null) return false;
        existing.FullName = obituary.FullName;
        existing.DateOfBirth = obituary.DateOfBirth;
        existing.DateOfDeath = obituary.DateOfDeath;
        existing.Biography = obituary.Biography;
        existing.PhotoUrl = obituary.PhotoUrl;
        existing.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return true;
    }
}
