using SyncServer.Core.Domain.Entities;

namespace SyncServer.Core.Interfaces;

public interface ISyncService
{
    Task<SyncResult> SyncAsync(Guid userId, Guid deviceId, SyncRequest request);
}

public class SyncRequest
{
    public List<Device> Devices { get; set; } = new();
    public List<Application> Applications { get; set; } = new();
    public List<Session> Sessions { get; set; } = new();
    public List<Activity> Activities { get; set; } = new();
    public List<Threshold> Thresholds { get; set; } = new();
    public List<UserSetting> Settings { get; set; } = new();
    public List<Category> Categories { get; set; } = new();
}

public class SyncResult
{
    public List<Device> Devices { get; set; } = new();
    public List<Application> Applications { get; set; } = new();
    public List<Session> Sessions { get; set; } = new();
    public List<Activity> Activities { get; set; } = new();
    public List<Threshold> Thresholds { get; set; } = new();
    public List<UserSetting> Settings { get; set; } = new();
    public List<Category> Categories { get; set; } = new();
    public DateTime ServerTime { get; set; }
}