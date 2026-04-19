using SyncServer.Core.Domain.Entities;

namespace SyncServer.Core.Interfaces;

public interface ISyncService
{
    Task<SyncResult> SyncAsync(Guid userId, Guid deviceId, SyncRequest request);
    Task<SyncPullResult> PullChangesAsync(Guid userId, DateTime since);
}

public class SyncRequest
{
    public DateTime LastSyncAt { get; set; }
    public Guid DeviceId { get; set; }
    public List<Session> Sessions { get; set; } = new();
    public List<Activity> Activities { get; set; } = new();
    public List<Threshold> Thresholds { get; set; } = new();
    public List<UserSetting> Settings { get; set; } = new();
    public List<Category> Categories { get; set; } = new();
    public List<Application> Applications { get; set; } = new();
}

public class SyncResult
{
    public List<Session> Sessions { get; set; } = new();
    public List<Activity> Activities { get; set; } = new();
    public List<Threshold> Thresholds { get; set; } = new();
    public List<UserSetting> Settings { get; set; } = new();
    public List<Category> Categories { get; set; } = new();
    public List<Application> Applications { get; set; } = new();
    public DateTime ServerTime { get; set; }
}

public class SyncPullResult
{
    public List<Session> Sessions { get; set; } = new();
    public List<Activity> Activities { get; set; } = new();
    public List<Threshold> Thresholds { get; set; } = new();
    public List<UserSetting> Settings { get; set; } = new();
    public List<Category> Categories { get; set; } = new();
    public List<Application> Applications { get; set; } = new();
    public DateTime ServerTime { get; set; }
}