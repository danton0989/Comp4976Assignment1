namespace Frontend.Blazor.Models;

public class ObituaryUpdateDto
{
    public string FullName { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public DateTime DateOfDeath { get; set; }
    public string Biography { get; set; } = string.Empty;
}