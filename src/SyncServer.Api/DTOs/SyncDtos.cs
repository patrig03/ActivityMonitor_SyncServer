namespace SyncServer.Api.DTOs;

public class SyncRequestDto
{
    public DateTime LastSyncAt { get; set; }
    public Guid DeviceId { get; set; }
    public List<SessionDto> Sessions { get; set; } = new();
    public List<ActivityDto> Activities { get; set; } = new();
    public List<ThresholdDto> Thresholds { get; set; } = new();
    public List<UserSettingDto> Settings { get; set; } = new();
    public List<CategoryDto> Categories { get; set; } = new();
    public List<ApplicationDto> Applications { get; set; } = new();
}

public class SyncResponseDto
{
    public List<SessionDto> Sessions { get; set; } = new();
    public List<ActivityDto> Activities { get; set; } = new();
    public List<ThresholdDto> Thresholds { get; set; } = new();
    public List<UserSettingDto> Settings { get; set; } = new();
    public List<CategoryDto> Categories { get; set; } = new();
    public List<ApplicationDto> Applications { get; set; } = new();
    public DateTime ServerTime { get; set; }
}

public class SyncPullResponseDto
{
    public List<SessionDto> Sessions { get; set; } = new();
    public List<ActivityDto> Activities { get; set; } = new();
    public List<ThresholdDto> Thresholds { get; set; } = new();
    public List<UserSettingDto> Settings { get; set; } = new();
    public List<CategoryDto> Categories { get; set; } = new();
    public List<ApplicationDto> Applications { get; set; } = new();
    public DateTime ServerTime { get; set; }
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
    public Guid? CategoryId { get; set; }
    public string? Url { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime Timestamp { get; set; }
    public int Duration { get; set; }
}

public class ThresholdDto
{
    public Guid Id { get; set; }
    public Guid? CategoryId { get; set; }
    public Guid? ApplicationId { get; set; }
    public bool Active { get; set; }
    public string TargetType { get; set; } = "Category";
    public string InterventionType { get; set; } = string.Empty;
    public string DurationType { get; set; } = "Daily";
    public int SessionLimitSec { get; set; }
    public int DailyLimitSec { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}

public class UserSettingDto
{
    public Guid Id { get; set; }
    public Guid DeviceId { get; set; }
    public int DeltaTimeSeconds { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CategoryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}

public class ApplicationDto
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
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
