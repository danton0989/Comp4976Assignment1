using Microsoft.AspNetCore.Components.Forms;

namespace Frontend.Blazor.Models;

public class ObituaryCreateDto
{
    public string FullName { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; } = DateTime.Now.AddYears(-70);
    public DateTime DateOfDeath { get; set; } = DateTime.Now;
    public string Biography { get; set; } = string.Empty;
    public IBrowserFile? PhotoFile { get; set; }
}