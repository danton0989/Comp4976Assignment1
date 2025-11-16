using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ObituaryApp.Models;
using ObituaryApp.Services;
using System.Security.Claims;

namespace ObituaryApp.Controllers;

[Authorize]
public class ObituaryController : Controller
{
    private readonly IObituaryService _service;

    public ObituaryController(IObituaryService service)
    {
        _service = service;
    }

    // GET: /Obituaries
    [AllowAnonymous]
    public async Task<IActionResult> Index(int page = 1, string? search = null)
    {
        var obituaries = await _service.GetAllAsync(page, 20, search);
        ViewData["Search"] = search;
        ViewData["Page"] = page;
        return View(obituaries);
    }

    // GET: /Obituaries/Details/5
    [AllowAnonymous]
    public async Task<IActionResult> Details(int id)
    {
        var obituary = await _service.GetByIdAsync(id);
        if (obituary == null)
        {
            return NotFound();
        }
        return View(obituary);
    }

    // GET: /Obituaries/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: /Obituaries/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Obituary obituary, IFormFile? photo)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.Identity?.Name ?? string.Empty;
        obituary.CreatorId = userId;
        obituary.CreatedAt = DateTime.UtcNow;

        // Remove validation errors for fields we're setting programmatically
        ModelState.Remove(nameof(obituary.CreatorId));
        ModelState.Remove(nameof(obituary.CreatedAt));

        if (ModelState.IsValid)
        {
            // Handle photo upload
            if (photo != null && photo.Length > 0)
            {
                var uploads = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "obituaries");
                if (!Directory.Exists(uploads))
                {
                    Directory.CreateDirectory(uploads);
                }
                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(photo.FileName)}";
                var filePath = Path.Combine(uploads, fileName);
                
                using (var stream = System.IO.File.Create(filePath))
                {
                    await photo.CopyToAsync(stream);
                }
                obituary.PhotoUrl = $"/images/obituaries/{fileName}";
            }

            await _service.CreateAsync(obituary);
            return RedirectToAction(nameof(Index));
        }
        return View(obituary);
    }

    // GET: /Obituaries/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var obituary = await _service.GetByIdAsync(id);
        if (obituary == null)
        {
            return NotFound();
        }

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.Identity?.Name ?? string.Empty;
        var isAdmin = User.IsInRole("admin");
        if (!isAdmin && obituary.CreatorId != userId)
        {
            return Forbid();
        }

        return View(obituary);
    }

    // POST: /Obituaries/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Obituary obituary, IFormFile? photo)
    {
        if (id != obituary.Id)
        {
            return NotFound();
        }

        var existing = await _service.GetByIdAsync(id);
        if (existing == null)
        {
            return NotFound();
        }

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.Identity?.Name ?? string.Empty;
        var isAdmin = User.IsInRole("admin");
        if (!isAdmin && existing.CreatorId != userId)
        {
            return Forbid();
        }

        // Set values that won't come from the form
        obituary.CreatorId = existing.CreatorId;
        obituary.CreatedAt = existing.CreatedAt;
        obituary.UpdatedAt = DateTime.UtcNow;

        // Remove validation errors for fields we're setting programmatically
        ModelState.Remove(nameof(obituary.CreatorId));
        ModelState.Remove(nameof(obituary.CreatedAt));
        ModelState.Remove(nameof(obituary.UpdatedAt));

        if (ModelState.IsValid)
        {
            // Handle photo upload
            if (photo != null && photo.Length > 0)
            {
                // Delete old photo if exists
                if (!string.IsNullOrEmpty(existing.PhotoUrl) && existing.PhotoUrl.StartsWith("/images/"))
                {
                    var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", existing.PhotoUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                    if (System.IO.File.Exists(oldPath))
                    {
                        try { System.IO.File.Delete(oldPath); } catch { /* ignore */ }
                    }
                }

                var uploads = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "obituaries");
                if (!Directory.Exists(uploads))
                {
                    Directory.CreateDirectory(uploads);
                }
                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(photo.FileName)}";
                var filePath = Path.Combine(uploads, fileName);
                
                using (var stream = System.IO.File.Create(filePath))
                {
                    await photo.CopyToAsync(stream);
                }
                obituary.PhotoUrl = $"/images/obituaries/{fileName}";
            }
            else
            {
                obituary.PhotoUrl = existing.PhotoUrl;
            }

            await _service.UpdateAsync(obituary);
            return RedirectToAction(nameof(Index));
        }
        return View(obituary);
    }

    // GET: /Obituaries/Delete/5
    public async Task<IActionResult> Delete(int id)
    {
        var obituary = await _service.GetByIdAsync(id);
        if (obituary == null)
        {
            return NotFound();
        }

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.Identity?.Name ?? string.Empty;
        var isAdmin = User.IsInRole("admin");
        if (!isAdmin && obituary.CreatorId != userId)
        {
            return Forbid();
        }

        return View(obituary);
    }

    // POST: /Obituaries/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var obituary = await _service.GetByIdAsync(id);
        if (obituary == null)
        {
            return NotFound();
        }

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.Identity?.Name ?? string.Empty;
        var isAdmin = User.IsInRole("admin");
        if (!isAdmin && obituary.CreatorId != userId)
        {
            return Forbid();
        }

        await _service.DeleteAsync(id);
        return RedirectToAction(nameof(Index));
    }
}
