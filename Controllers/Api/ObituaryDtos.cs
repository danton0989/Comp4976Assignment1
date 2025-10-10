using ObituaryApp.Models;

namespace ObituaryApp.Controllers.Api;

public record ObituaryCreateDto(string FullName, DateOnly DateOfBirth, DateOnly DateOfDeath, string Biography);
public record ObituaryUpdateDto(string FullName, DateOnly DateOfBirth, DateOnly DateOfDeath, string Biography, string? PhotoUrl);

public class ObituaryCreateForm
{
	public string FullName { get; set; } = null!;
	public DateOnly DateOfBirth { get; set; }
	public DateOnly DateOfDeath { get; set; }
	public string Biography { get; set; } = null!;
	public IFormFile? Photo { get; set; }
}
