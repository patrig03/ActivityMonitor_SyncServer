namespace SyncServer.Core.Domain.Entities;

public class Application
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public Guid? CategoryId { get; set; }
    public string? WindowTitle { get; set; }
    public string? ClassName { get; set; }
    public string? ProcessName { get; set; }
    public int? PositionX { get; set; }
    public int? PositionY { get; set; }
    public int? Width { get; set; }
    public int? Height { get; set; }
    public int? WindowId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}
