namespace Nucleus.Core.Models;

public class NucleusLog
{
    public Guid? Id { get; set; } 
    public DateTime Timestamp { get; set; } 
    public long DurationMs { get; set; } 
    public int StatusCode { get; set; } 
    public string Path { get; set; } = string.Empty;
}