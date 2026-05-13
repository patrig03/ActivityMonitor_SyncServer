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
        if (!Guid.TryParse(request.DeviceId, out var deviceGuid) || deviceGuid == Guid.Empty)
        {
            return BadRequest(new { message = "DeviceId is required" });
        }

        var device = await _deviceService.GetByIdAsync(deviceGuid);
        if (device == null || device.UserId != userId)
        {
            return BadRequest(new { message = "DeviceId is not registered for the current user" });
        }

        var syncRequest = ToSyncRequest(request, deviceGuid);
        var result = await _syncService.SyncAsync(userId, deviceGuid, syncRequest);
        await _deviceService.UpdateLastSyncAsync(deviceGuid);

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

    private static SyncRequest ToSyncRequest(SyncRequestDto dto, Guid deviceGuid)
    {
        var syncTimestamp = DateTime.UtcNow;

        return new SyncRequest
        {
            LastSyncAt = NormalizeTimestamp(dto.LastSyncAt, DateTime.UnixEpoch),
            DeviceId = deviceGuid,
            Sessions = dto.Sessions.Select(s => new Session 
            { 
                Id = ParseGuidOrEmpty(s.Id), 
                UserId = Guid.Empty,
                DeviceId = ParseGuidOrEmpty(s.DeviceId) == Guid.Empty ? deviceGuid : ParseGuidOrEmpty(s.DeviceId), 
                ApplicationId = ParseGuidOrEmpty(s.ApplicationId), 
                StartTime = NormalizeTimestamp(s.StartTime, syncTimestamp), 
                EndTime = s.EndTime.HasValue ? NormalizeTimestamp(s.EndTime.Value, syncTimestamp) : null, 
                Duration = s.Duration, 
                CreatedAt = NormalizeTimestamp(s.CreatedAt, syncTimestamp) 
            }).ToList(),
            Activities = dto.Activities.Select(a => new Activity 
            { 
                Id = ParseGuidOrEmpty(a.Id), 
                UserId = Guid.Empty,
                DeviceId = ParseGuidOrEmpty(a.DeviceId) == Guid.Empty ? deviceGuid : ParseGuidOrEmpty(a.DeviceId), 
                ApplicationId = ParseGuidOrEmpty(a.ApplicationId), 
                CategoryId = ParseGuidOrEmpty(a.CategoryId),
                Url = a.Url, 
                Timestamp = NormalizeTimestamp(a.Timestamp, a.CreatedAt, syncTimestamp), 
                Duration = Math.Max(0, a.Duration), 
                CreatedAt = NormalizeTimestamp(a.CreatedAt, syncTimestamp) 
            }).ToList(),
            Thresholds = dto.Thresholds.Select(t => new Threshold 
            { 
                Id = ParseGuidOrEmpty(t.Id), 
                UserId = Guid.Empty,
                CategoryId = ParseGuidOrEmpty(t.CategoryId), 
                ApplicationId = ParseGuidOrEmpty(t.ApplicationId),
                TargetType = string.IsNullOrWhiteSpace(t.TargetType) ? "Category" : t.TargetType.Trim(),
                DailyLimitSec = t.DailyLimitSec, 
                DurationType = string.IsNullOrWhiteSpace(t.DurationType) ? "Daily" : t.DurationType.Trim(),
                SessionLimitSec = Math.Max(0, t.SessionLimitSec),
                InterventionType = t.InterventionType, 
                Active = t.Active, 
                UpdatedAt = NormalizeTimestamp(t.UpdatedAt, t.CreatedAt, syncTimestamp), 
                DeletedAt = t.DeletedAt 
            }).ToList(),
            Settings = dto.Settings.Select(s => ToUserSetting(s, deviceGuid, syncTimestamp)).ToList(),
            Categories = dto.Categories.Select(c => new Category 
            { 
                Id = ParseGuidOrEmpty(c.Id), 
                UserId = Guid.Empty,
                Name = c.Name, 
                Description = c.Description,
                CreatedAt = NormalizeTimestamp(c.CreatedAt, syncTimestamp), 
                UpdatedAt = NormalizeTimestamp(c.UpdatedAt, c.CreatedAt, syncTimestamp), 
                DeletedAt = c.DeletedAt 
            }).ToList(),
            Applications = dto.Applications.Select(a => new Application 
            { 
                Id = ParseGuidOrEmpty(a.Id), 
                UserId = Guid.Empty,
                Name = ResolveApplicationName(a), 
                CategoryId = ParseGuidOrEmpty(a.CategoryId), 
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
            Sessions = result.Sessions.Select(s => new SessionDto { Id = s.Id.ToString(), DeviceId = s.DeviceId.ToString(), ApplicationId = s.ApplicationId.ToString(), StartTime = s.StartTime, EndTime = s.EndTime, Duration = s.Duration, CreatedAt = s.CreatedAt }).ToList(),
            Activities = result.Activities.Select(a => new ActivityDto { Id = a.Id.ToString(), DeviceId = a.DeviceId.ToString(), ApplicationId = a.ApplicationId.ToString(), CategoryId = a.CategoryId?.ToString(), Url = a.Url, CreatedAt = a.CreatedAt, Timestamp = a.Timestamp, Duration = a.Duration }).ToList(),
            Thresholds = result.Thresholds.Select(t => new ThresholdDto { Id = t.Id.ToString(), CategoryId = t.CategoryId?.ToString(), ApplicationId = t.ApplicationId?.ToString(), Active = t.Active, TargetType = t.TargetType, InterventionType = t.InterventionType, DurationType = t.DurationType, SessionLimitSec = t.SessionLimitSec, DailyLimitSec = t.DailyLimitSec, CreatedAt = t.UpdatedAt, UpdatedAt = t.UpdatedAt, DeletedAt = t.DeletedAt }).ToList(),
            Settings = result.Settings.Select(ToUserSettingDto).Where(s => s != null).Select(s => s!).ToList(),
            Categories = result.Categories.Select(c => new CategoryDto { Id = c.Id.ToString(), Name = c.Name, Description = c.Description, CreatedAt = c.CreatedAt, UpdatedAt = c.UpdatedAt, DeletedAt = c.DeletedAt }).ToList(),
            Applications = result.Applications.Select(a => new ApplicationDto { Id = a.Id.ToString(), Name = a.Name, CategoryId = a.CategoryId?.ToString(), WindowTitle = a.WindowTitle, ClassName = a.ClassName, ProcessName = a.ProcessName, PositionX = a.PositionX, PositionY = a.PositionY, Width = a.Width, Height = a.Height, WindowId = a.WindowId, CreatedAt = a.CreatedAt, UpdatedAt = a.UpdatedAt, DeletedAt = a.DeletedAt }).ToList(),
            ServerTime = result.ServerTime
        };
    }

    private static SyncPullResponseDto ToPullResponse(SyncPullResult result)
    {
        return new SyncPullResponseDto
        {
            Sessions = result.Sessions.Select(s => new SessionDto { Id = s.Id.ToString(), DeviceId = s.DeviceId.ToString(), ApplicationId = s.ApplicationId.ToString(), StartTime = s.StartTime, EndTime = s.EndTime, Duration = s.Duration, CreatedAt = s.CreatedAt }).ToList(),
            Activities = result.Activities.Select(a => new ActivityDto { Id = a.Id.ToString(), DeviceId = a.DeviceId.ToString(), ApplicationId = a.ApplicationId.ToString(), CategoryId = a.CategoryId?.ToString(), Url = a.Url, CreatedAt = a.CreatedAt, Timestamp = a.Timestamp, Duration = a.Duration }).ToList(),
            Thresholds = result.Thresholds.Select(t => new ThresholdDto { Id = t.Id.ToString(), CategoryId = t.CategoryId?.ToString(), ApplicationId = t.ApplicationId?.ToString(), Active = t.Active, TargetType = t.TargetType, InterventionType = t.InterventionType, DurationType = t.DurationType, SessionLimitSec = t.SessionLimitSec, DailyLimitSec = t.DailyLimitSec, CreatedAt = t.UpdatedAt, UpdatedAt = t.UpdatedAt, DeletedAt = t.DeletedAt }).ToList(),
            Settings = result.Settings.Select(ToUserSettingDto).Where(s => s != null).Select(s => s!).ToList(),
            Categories = result.Categories.Select(c => new CategoryDto { Id = c.Id.ToString(), Name = c.Name, Description = c.Description, CreatedAt = c.CreatedAt, UpdatedAt = c.UpdatedAt, DeletedAt = c.DeletedAt }).ToList(),
            Applications = result.Applications.Select(a => new ApplicationDto { Id = a.Id.ToString(), Name = a.Name, CategoryId = a.CategoryId?.ToString(), WindowTitle = a.WindowTitle, ClassName = a.ClassName, ProcessName = a.ProcessName, PositionX = a.PositionX, PositionY = a.PositionY, Width = a.Width, Height = a.Height, WindowId = a.WindowId, CreatedAt = a.CreatedAt, UpdatedAt = a.UpdatedAt, DeletedAt = a.DeletedAt }).ToList(),
            ServerTime = result.ServerTime
        };
    }

    private static UserSetting ToUserSetting(UserSettingDto dto, Guid fallbackDeviceId, DateTime syncTimestamp)
    {
        var dtoDeviceId = ParseGuidOrEmpty(dto.DeviceId);
        var deviceId = dtoDeviceId == Guid.Empty ? fallbackDeviceId : dtoDeviceId;
        var dtoId = ParseGuidOrEmpty(dto.Id);
        return new UserSetting
        {
            Id = dtoId == Guid.Empty ? deviceId : dtoId,
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
            Id = setting.Id.ToString(),
            DeviceId = deviceId.ToString(),
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

    private static Guid ParseGuidOrEmpty(string? value)
    {
        return string.IsNullOrWhiteSpace(value) || !Guid.TryParse(value, out var result) ? Guid.Empty : result;
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
