namespace Frontend.Blazor.Models;

public class Obituary
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public DateTime DateOfDeath { get; set; }
    public string Biography { get; set; } = string.Empty;
    public string? PhotoUrl { get; set; }
    public string CreatorId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
