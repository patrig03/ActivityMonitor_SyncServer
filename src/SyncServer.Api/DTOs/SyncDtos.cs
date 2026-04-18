using SyncServer.Core.Domain.Entities;

namespace SyncServer.Api.DTOs;

public class SyncRequestDto
{
    public List<DeviceDto> Devices { get; set; } = new();
    public List<ApplicationDto> Applications { get; set; } = new();
    public List<SessionDto> Sessions { get; set; } = new();
    public List<ActivityDto> Activities { get; set; } = new();
    public List<ThresholdDto> Thresholds { get; set; } = new();
    public List<UserSettingDto> Settings { get; set; } = new();
    public List<CategoryDto> Categories { get; set; } = new();
}

public class SyncResponseDto
{
    public List<DeviceDto> Devices { get; set; } = new();
    public List<ApplicationDto> Applications { get; set; } = new();
    public List<SessionDto> Sessions { get; set; } = new();
    public List<ActivityDto> Activities { get; set; } = new();
    public List<ThresholdDto> Thresholds { get; set; } = new();
    public List<UserSettingDto> Settings { get; set; } = new();
    public List<CategoryDto> Categories { get; set; } = new();
    public DateTime ServerTime { get; set; }
}

public class DeviceDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime LastSyncAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class ApplicationDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Guid? CategoryId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}

public class SessionDto
{
    public Guid Id { get; set; }
    public Guid DeviceId { get; set; }
    public Guid ApplicationId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public int Duration { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ActivityDto
{
    public Guid Id { get; set; }
    public Guid DeviceId { get; set; }
    public Guid ApplicationId { get; set; }
    public string? Url { get; set; }
    public DateTime Timestamp { get; set; }
    public int Duration { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ThresholdDto
{
    public Guid Id { get; set; }
    public Guid? CategoryId { get; set; }
    public int DailyLimitSec { get; set; }
    public string InterventionType { get; set; } = string.Empty;
    public bool Active { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}

public class UserSettingDto
{
    public Guid Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; }
}

public class CategoryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}