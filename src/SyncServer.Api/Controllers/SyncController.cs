using System.Globalization;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SyncServer.Api.DTOs;
using SyncServer.Core.Domain.Entities;
using SyncServer.Core.Interfaces;

namespace SyncServer.Api.Controllers;

[ApiController]
[Route("api/sync")]
[Authorize]
public class SyncController : ControllerBase
{
    private const string DeltaTimeSecondsKeyPrefix = "delta_time_seconds:";

    private readonly IDeviceService _deviceService;
    private readonly ISyncService _syncService;

    public SyncController(ISyncService syncService, IDeviceService deviceService)
    {
        _syncService = syncService;
        _deviceService = deviceService;
    }

    [HttpPost]
    public async Task<ActionResult<SyncResponseDto>> Sync([FromBody] SyncRequestDto request)
    {
        var userId = GetUserId();
        if (request.DeviceId == Guid.Empty)
        {
            return BadRequest(new { message = "DeviceId is required" });
        }

        var device = await _deviceService.GetByIdAsync(request.DeviceId);
        if (device == null || device.UserId != userId)
        {
            return BadRequest(new { message = "DeviceId is not registered for the current user" });
        }

        var syncRequest = ToSyncRequest(request);
        var result = await _syncService.SyncAsync(userId, request.DeviceId, syncRequest);
        await _deviceService.UpdateLastSyncAsync(request.DeviceId);

        return Ok(ToSyncResponse(result));
    }

    [HttpGet("pull")]
    public async Task<ActionResult<SyncPullResponseDto>> PullChanges([FromQuery] DateTime since)
    {
        var userId = GetUserId();
        var result = await _syncService.PullChangesAsync(userId, since);

        return Ok(ToPullResponse(result));
    }

    private Guid GetUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(claim!);
    }

    private static SyncRequest ToSyncRequest(SyncRequestDto dto)
    {
        var syncTimestamp = DateTime.UtcNow;

        return new SyncRequest
        {
            LastSyncAt = NormalizeTimestamp(dto.LastSyncAt, DateTime.UnixEpoch),
            DeviceId = dto.DeviceId,
            Sessions = dto.Sessions.Select(s => new Session 
            { 
                Id = s.Id, 
                UserId = Guid.Empty,
                DeviceId = s.DeviceId == Guid.Empty ? dto.DeviceId : s.DeviceId, 
                ApplicationId = s.ApplicationId, 
                StartTime = NormalizeTimestamp(s.StartTime, syncTimestamp), 
                EndTime = s.EndTime.HasValue ? NormalizeTimestamp(s.EndTime.Value, syncTimestamp) : null, 
                Duration = s.Duration, 
                CreatedAt = NormalizeTimestamp(s.CreatedAt, syncTimestamp) 
            }).ToList(),
            Activities = dto.Activities.Select(a => new Activity 
            { 
                Id = a.Id, 
                UserId = Guid.Empty,
                DeviceId = a.DeviceId == Guid.Empty ? dto.DeviceId : a.DeviceId, 
                ApplicationId = a.ApplicationId, 
                CategoryId = a.CategoryId,
                Url = a.Url, 
                Timestamp = NormalizeTimestamp(a.Timestamp, a.CreatedAt, syncTimestamp), 
                Duration = Math.Max(0, a.Duration), 
                CreatedAt = NormalizeTimestamp(a.CreatedAt, syncTimestamp) 
            }).ToList(),
            Thresholds = dto.Thresholds.Select(t => new Threshold 
            { 
                Id = t.Id, 
                UserId = Guid.Empty,
                CategoryId = t.CategoryId, 
                ApplicationId = t.ApplicationId,
                TargetType = string.IsNullOrWhiteSpace(t.TargetType) ? "Category" : t.TargetType.Trim(),
                DailyLimitSec = t.DailyLimitSec, 
                DurationType = string.IsNullOrWhiteSpace(t.DurationType) ? "Daily" : t.DurationType.Trim(),
                SessionLimitSec = Math.Max(0, t.SessionLimitSec),
                InterventionType = t.InterventionType, 
                Active = t.Active, 
                UpdatedAt = NormalizeTimestamp(t.UpdatedAt, t.CreatedAt, syncTimestamp), 
                DeletedAt = t.DeletedAt 
            }).ToList(),
            Settings = dto.Settings.Select(s => ToUserSetting(s, dto.DeviceId, syncTimestamp)).ToList(),
            Categories = dto.Categories.Select(c => new Category 
            { 
                Id = c.Id, 
                UserId = Guid.Empty,
                Name = c.Name, 
                Description = c.Description,
                CreatedAt = NormalizeTimestamp(c.CreatedAt, syncTimestamp), 
                UpdatedAt = NormalizeTimestamp(c.UpdatedAt, c.CreatedAt, syncTimestamp), 
                DeletedAt = c.DeletedAt 
            }).ToList(),
            Applications = dto.Applications.Select(a => new Application 
            { 
                Id = a.Id, 
                UserId = Guid.Empty,
                Name = ResolveApplicationName(a), 
                CategoryId = a.CategoryId, 
                WindowTitle = a.WindowTitle,
                ClassName = a.ClassName,
                ProcessName = a.ProcessName,
                PositionX = a.PositionX,
                PositionY = a.PositionY,
                Width = a.Width,
                Height = a.Height,
                WindowId = a.WindowId,
                CreatedAt = NormalizeTimestamp(a.CreatedAt, syncTimestamp), 
                UpdatedAt = NormalizeTimestamp(a.UpdatedAt, a.CreatedAt, syncTimestamp), 
                DeletedAt = a.DeletedAt 
            }).ToList()
        };
    }

    private static SyncResponseDto ToSyncResponse(SyncResult result)
    {
        return new SyncResponseDto
        {
            Sessions = result.Sessions.Select(s => new SessionDto { Id = s.Id, DeviceId = s.DeviceId, ApplicationId = s.ApplicationId, StartTime = s.StartTime, EndTime = s.EndTime, Duration = s.Duration, CreatedAt = s.CreatedAt }).ToList(),
            Activities = result.Activities.Select(a => new ActivityDto { Id = a.Id, DeviceId = a.DeviceId, ApplicationId = a.ApplicationId, CategoryId = a.CategoryId, Url = a.Url, CreatedAt = a.CreatedAt, Timestamp = a.Timestamp, Duration = a.Duration }).ToList(),
            Thresholds = result.Thresholds.Select(t => new ThresholdDto { Id = t.Id, CategoryId = t.CategoryId, ApplicationId = t.ApplicationId, Active = t.Active, TargetType = t.TargetType, InterventionType = t.InterventionType, DurationType = t.DurationType, SessionLimitSec = t.SessionLimitSec, DailyLimitSec = t.DailyLimitSec, CreatedAt = t.UpdatedAt, UpdatedAt = t.UpdatedAt, DeletedAt = t.DeletedAt }).ToList(),
            Settings = result.Settings.Select(ToUserSettingDto).Where(s => s != null).Select(s => s!).ToList(),
            Categories = result.Categories.Select(c => new CategoryDto { Id = c.Id, Name = c.Name, Description = c.Description, CreatedAt = c.CreatedAt, UpdatedAt = c.UpdatedAt, DeletedAt = c.DeletedAt }).ToList(),
            Applications = result.Applications.Select(a => new ApplicationDto { Id = a.Id, Name = a.Name, CategoryId = a.CategoryId, WindowTitle = a.WindowTitle, ClassName = a.ClassName, ProcessName = a.ProcessName, PositionX = a.PositionX, PositionY = a.PositionY, Width = a.Width, Height = a.Height, WindowId = a.WindowId, CreatedAt = a.CreatedAt, UpdatedAt = a.UpdatedAt, DeletedAt = a.DeletedAt }).ToList(),
            ServerTime = result.ServerTime
        };
    }

    private static SyncPullResponseDto ToPullResponse(SyncPullResult result)
    {
        return new SyncPullResponseDto
        {
            Sessions = result.Sessions.Select(s => new SessionDto { Id = s.Id, DeviceId = s.DeviceId, ApplicationId = s.ApplicationId, StartTime = s.StartTime, EndTime = s.EndTime, Duration = s.Duration, CreatedAt = s.CreatedAt }).ToList(),
            Activities = result.Activities.Select(a => new ActivityDto { Id = a.Id, DeviceId = a.DeviceId, ApplicationId = a.ApplicationId, CategoryId = a.CategoryId, Url = a.Url, CreatedAt = a.CreatedAt, Timestamp = a.Timestamp, Duration = a.Duration }).ToList(),
            Thresholds = result.Thresholds.Select(t => new ThresholdDto { Id = t.Id, CategoryId = t.CategoryId, ApplicationId = t.ApplicationId, Active = t.Active, TargetType = t.TargetType, InterventionType = t.InterventionType, DurationType = t.DurationType, SessionLimitSec = t.SessionLimitSec, DailyLimitSec = t.DailyLimitSec, CreatedAt = t.UpdatedAt, UpdatedAt = t.UpdatedAt, DeletedAt = t.DeletedAt }).ToList(),
            Settings = result.Settings.Select(ToUserSettingDto).Where(s => s != null).Select(s => s!).ToList(),
            Categories = result.Categories.Select(c => new CategoryDto { Id = c.Id, Name = c.Name, Description = c.Description, CreatedAt = c.CreatedAt, UpdatedAt = c.UpdatedAt, DeletedAt = c.DeletedAt }).ToList(),
            Applications = result.Applications.Select(a => new ApplicationDto { Id = a.Id, Name = a.Name, CategoryId = a.CategoryId, WindowTitle = a.WindowTitle, ClassName = a.ClassName, ProcessName = a.ProcessName, PositionX = a.PositionX, PositionY = a.PositionY, Width = a.Width, Height = a.Height, WindowId = a.WindowId, CreatedAt = a.CreatedAt, UpdatedAt = a.UpdatedAt, DeletedAt = a.DeletedAt }).ToList(),
            ServerTime = result.ServerTime
        };
    }

    private static UserSetting ToUserSetting(UserSettingDto dto, Guid fallbackDeviceId, DateTime syncTimestamp)
    {
        var deviceId = dto.DeviceId == Guid.Empty ? fallbackDeviceId : dto.DeviceId;
        return new UserSetting
        {
            Id = deviceId == Guid.Empty ? dto.Id : deviceId,
            UserId = Guid.Empty,
            Key = BuildDeltaTimeKey(deviceId),
            Value = dto.DeltaTimeSeconds.ToString(CultureInfo.InvariantCulture),
            UpdatedAt = NormalizeTimestamp(dto.UpdatedAt, syncTimestamp)
        };
    }

    private static UserSettingDto? ToUserSettingDto(UserSetting setting)
    {
        if (!TryParseDeltaTimeSetting(setting, out var deviceId, out var deltaTimeSeconds))
        {
            return null;
        }

        return new UserSettingDto
        {
            Id = setting.Id,
            DeviceId = deviceId,
            DeltaTimeSeconds = deltaTimeSeconds,
            UpdatedAt = setting.UpdatedAt
        };
    }

    private static bool TryParseDeltaTimeSetting(UserSetting setting, out Guid deviceId, out int deltaTimeSeconds)
    {
        deviceId = Guid.Empty;
        deltaTimeSeconds = 0;

        if (string.IsNullOrWhiteSpace(setting.Key) ||
            !setting.Key.StartsWith(DeltaTimeSecondsKeyPrefix, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var deviceToken = setting.Key[DeltaTimeSecondsKeyPrefix.Length..];
        if (!Guid.TryParse(deviceToken, out deviceId))
        {
            return false;
        }

        return int.TryParse(setting.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out deltaTimeSeconds);
    }

    private static string BuildDeltaTimeKey(Guid deviceId)
    {
        return $"{DeltaTimeSecondsKeyPrefix}{deviceId:D}";
    }

    private static string ResolveApplicationName(ApplicationDto dto)
    {
        return FirstNonEmpty(dto.ProcessName, dto.ClassName, dto.WindowTitle, dto.Name) ?? "Unknown";
    }

    private static string? FirstNonEmpty(params string?[] values)
    {
        return values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value))?.Trim();
    }

    private static DateTime NormalizeTimestamp(DateTime candidate, DateTime fallback)
    {
        return NormalizeTimestamp(candidate, default(DateTime), fallback);
    }

    private static DateTime NormalizeTimestamp(DateTime candidate, DateTime secondaryFallback, DateTime fallback)
    {
        if (candidate != default)
        {
            return candidate.Kind == DateTimeKind.Utc ? candidate : candidate.ToUniversalTime();
        }

        if (secondaryFallback != default)
        {
            return secondaryFallback.Kind == DateTimeKind.Utc ? secondaryFallback : secondaryFallback.ToUniversalTime();
        }

        return fallback;
    }
}
