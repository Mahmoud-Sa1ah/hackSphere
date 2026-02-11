namespace PentestHub.API.Data.Models;

public class Lab
{
    public int LabId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Difficulty { get; set; }
    public string? Category { get; set; }
    public string? LabType { get; set; }
    public int Points { get; set; }
}

