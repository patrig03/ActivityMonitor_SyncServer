namespace SyncServer.Core.Domain.Entities;

public class Activity
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid DeviceId { get; set; }
    public Guid ApplicationId { get; set; }
    public Guid? CategoryId { get; set; }
    public string? Url { get; set; }
    public DateTime Timestamp { get; set; }
    public int Duration { get; set; }
    public DateTime CreatedAt { get; set; }
}
