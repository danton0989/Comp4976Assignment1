namespace ObituaryApp.Models;

public class Obituary
{
    public int Id { get; set; }
    public string FullName { get; set; } = null!;
    public DateOnly DateOfBirth { get; set; }
    public DateOnly DateOfDeath { get; set; }
    public string Biography { get; set; } = null!;
    public string? PhotoUrl { get; set; }
    public string CreatorId { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
