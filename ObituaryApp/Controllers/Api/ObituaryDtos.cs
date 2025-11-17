using ObituaryApp.Models;

namespace ObituaryApp.Controllers.Api;

public record ObituaryCreateDto(string FullName, DateTime DateOfBirth, DateTime DateOfDeath, string Biography);
public record ObituaryUpdateDto(string FullName, DateTime DateOfBirth, DateTime DateOfDeath, string Biography, string? PhotoUrl);

public class ObituaryCreateForm
{
	public string FullName { get; set; } = null!;
	public DateTime DateOfBirth { get; set; }
	public DateTime DateOfDeath { get; set; }
	public string Biography { get; set; } = null!;
	public IFormFile? Photo { get; set; }
}
