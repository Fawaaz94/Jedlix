namespace Jedlix.Application.Models;

public class InputData
{
    public required string StartingTime { get; set; }
    public required UserSettings UserSettings { get; set; }
    public required CarData CarData { get; set; }
}