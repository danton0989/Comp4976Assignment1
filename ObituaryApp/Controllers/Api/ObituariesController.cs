using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ObituaryApp.Models;
using ObituaryApp.Services;
using System.Security.Claims;

namespace ObituaryApp.Controllers.Api;

[Route("api/[controller]")]
[ApiController]
public class ObituariesController : ControllerBase
{
    private readonly IObituaryService _service;

    public ObituariesController(IObituaryService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? search = null)
    {
        var list = await _service.GetAllAsync(page, pageSize, search);
        return Ok(list);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var item = await _service.GetByIdAsync(id);
        if (item == null) return NotFound();
        return Ok(item);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromForm] ObituaryCreateForm dto)
    {
        var userId = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User?.Identity?.Name ?? string.Empty;

        // validate dates
        if (dto.DateOfBirth > dto.DateOfDeath)
            return BadRequest("DateOfBirth must be before or equal to DateOfDeath");

        string? photoUrl = null;
        if (dto.Photo != null)
        {
            // validate by content-type, size and magic-bytes
            if (dto.Photo.Length > 5 * 1024 * 1024) return BadRequest("Image size must be <= 5MB");
            if (!TryValidateImage(dto.Photo, out var detectedExt, out var validationError))
                return BadRequest(validationError);

            var uploads = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "obituaries");
            if (!Directory.Exists(uploads)) Directory.CreateDirectory(uploads);
            var fileName = $"{Guid.NewGuid()}{detectedExt}";
            var filePath = Path.Combine(uploads, fileName);
            await using (var stream = System.IO.File.Create(filePath))
            {
                await dto.Photo.CopyToAsync(stream);
            }
            photoUrl = $"/images/obituaries/{fileName}";
        }

        var ob = new Obituary
        {
            FullName = dto.FullName,
            DateOfBirth = dto.DateOfBirth,
            DateOfDeath = dto.DateOfDeath,
            Biography = dto.Biography,
            CreatorId = userId ?? string.Empty,
            CreatedAt = DateTime.UtcNow,
            PhotoUrl = photoUrl
        };

        var created = await _service.CreateAsync(ob);
        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }

    private static bool IsImage(string contentType)
    {
        var ct = contentType.ToLowerInvariant();
        return ct == "image/jpeg" || ct == "image/jpg" || ct == "image/png";
    }

    // Validate by reading magic-bytes and return a recommended extension
    private static bool TryValidateImage(IFormFile file, out string detectedExtension, out string? error)
    {
        detectedExtension = string.Empty;
        error = null;
        try
        {
            using var ms = new MemoryStream();
            file.CopyTo(ms);
            var data = ms.ToArray();
            // JPEG magic: FF D8 FF
            if (data.Length >= 3 && data[0] == 0xFF && data[1] == 0xD8 && data[2] == 0xFF)
            {
                detectedExtension = ".jpg";
                return true;
            }
            // PNG magic: 89 50 4E 47 0D 0A 1A 0A
            if (data.Length >= 8 && data[0] == 0x89 && data[1] == 0x50 && data[2] == 0x4E && data[3] == 0x47)
            {
                detectedExtension = ".png";
                return true;
            }
            error = "Only JPEG/PNG images are allowed";
            return false;
        }
        catch (Exception ex)
        {
            error = "Invalid image file" + (ex.Message.Length > 0 ? $": {ex.Message}" : string.Empty);
            return false;
        }
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> Update(int id, [FromBody] ObituaryUpdateDto dto)
    {
        var existing = await _service.GetByIdAsync(id);
        if (existing == null) return NotFound();

        // authorization: only creator or admin (check roles via User.IsInRole("admin"))
        var userId = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User?.Identity?.Name ?? string.Empty;
        var isAdmin = User?.IsInRole("admin") ?? false;
        if (!isAdmin && existing.CreatorId != userId)
            return Forbid();

        existing.FullName = dto.FullName;
        existing.DateOfBirth = dto.DateOfBirth;
        existing.DateOfDeath = dto.DateOfDeath;
        existing.Biography = dto.Biography;
        // if photo changed, consider deleting old file (service will handle physical removal if needed)
        if (!string.Equals(existing.PhotoUrl, dto.PhotoUrl, StringComparison.OrdinalIgnoreCase))
        {
            // delete old file if it exists
            if (!string.IsNullOrEmpty(existing.PhotoUrl) && existing.PhotoUrl.StartsWith("/images/"))
            {
                var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", existing.PhotoUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                if (System.IO.File.Exists(oldPath))
                {
                    try { System.IO.File.Delete(oldPath); } catch { /* ignore */ }
                }
            }
            existing.PhotoUrl = dto.PhotoUrl;
        }

        var ok = await _service.UpdateAsync(existing);
        if (!ok) return BadRequest();
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> Delete(int id)
    {
        var existing = await _service.GetByIdAsync(id);
        if (existing == null) return NotFound();
        var userId = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User?.Identity?.Name ?? string.Empty;
        var isAdmin = User?.IsInRole("admin") ?? false;
        if (!isAdmin && existing.CreatorId != userId)
            return Forbid();

        var ok = await _service.DeleteAsync(id);
        if (!ok) return BadRequest();
        return NoContent();
    }
}
