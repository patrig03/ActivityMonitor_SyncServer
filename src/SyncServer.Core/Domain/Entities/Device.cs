namespace SyncServer.Core.Domain.Entities;

public class Device
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Platform { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime LastKnownActivity { get; set; }
    public DateTime LastSyncAt { get; set; }
    public DateTime CreatedAt { get; set; }
}