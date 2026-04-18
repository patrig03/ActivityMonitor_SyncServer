using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SyncServer.Api.DTOs;
using SyncServer.Core.Interfaces;
using SyncServer.Core.Domain.Entities;

namespace SyncServer.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SyncController : ControllerBase
{
    private readonly ISyncService _syncService;

    public SyncController(ISyncService syncService)
    {
        _syncService = syncService;
    }

    [HttpPost]
    public async Task<ActionResult<SyncResponseDto>> Sync([FromBody] SyncRequestDto request)
    {
        var userId = GetUserId();
        var deviceId = GetDeviceId();

        var syncRequest = ToSyncRequest(request);
        var result = await _syncService.SyncAsync(userId, deviceId, syncRequest);

        return Ok(ToSyncResponse(result));
    }

    private Guid GetUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(claim!);
    }

    private Guid GetDeviceId()
    {
        var claim = User.FindFirst("device_id")?.Value;
        return claim != null ? Guid.Parse(claim) : Guid.Empty;
    }

    private static SyncRequest ToSyncRequest(SyncRequestDto dto)
    {
        return new SyncRequest
        {
            Devices = dto.Devices.Select(d => new Device { Id = d.Id, Name = d.Name, LastSyncAt = d.LastSyncAt, CreatedAt = d.CreatedAt }).ToList(),
            Applications = dto.Applications.Select(a => new Application { Id = a.Id, Name = a.Name, CategoryId = a.CategoryId, CreatedAt = a.CreatedAt, UpdatedAt = a.UpdatedAt, DeletedAt = a.DeletedAt }).ToList(),
            Sessions = dto.Sessions.Select(s => new Session { Id = s.Id, DeviceId = s.DeviceId, ApplicationId = s.ApplicationId, StartTime = s.StartTime, EndTime = s.EndTime, Duration = s.Duration, CreatedAt = s.CreatedAt }).ToList(),
            Activities = dto.Activities.Select(a => new Activity { Id = a.Id, DeviceId = a.DeviceId, ApplicationId = a.ApplicationId, Url = a.Url, Timestamp = a.Timestamp, Duration = a.Duration, CreatedAt = a.CreatedAt }).ToList(),
            Thresholds = dto.Thresholds.Select(t => new Threshold { Id = t.Id, CategoryId = t.CategoryId, DailyLimitSec = t.DailyLimitSec, InterventionType = t.InterventionType, Active = t.Active, UpdatedAt = t.UpdatedAt, DeletedAt = t.DeletedAt }).ToList(),
            Settings = dto.Settings.Select(s => new UserSetting { Id = s.Id, Key = s.Key, Value = s.Value, UpdatedAt = s.UpdatedAt }).ToList(),
            Categories = dto.Categories.Select(c => new Core.Domain.Entities.Category { Id = c.Id, Name = c.Name, CreatedAt = c.CreatedAt, UpdatedAt = c.UpdatedAt, DeletedAt = c.DeletedAt }).ToList()
        };
    }

    private static SyncResponseDto ToSyncResponse(SyncResult result)
    {
        return new SyncResponseDto
        {
            Devices = result.Devices.Select(d => new DeviceDto { Id = d.Id, Name = d.Name, LastSyncAt = d.LastSyncAt, CreatedAt = d.CreatedAt, UpdatedAt = d.CreatedAt }).ToList(),
            Applications = result.Applications.Select(a => new ApplicationDto { Id = a.Id, Name = a.Name, CategoryId = a.CategoryId, CreatedAt = a.CreatedAt, UpdatedAt = a.UpdatedAt, DeletedAt = a.DeletedAt }).ToList(),
            Sessions = result.Sessions.Select(s => new SessionDto { Id = s.Id, DeviceId = s.DeviceId, ApplicationId = s.ApplicationId, StartTime = s.StartTime, EndTime = s.EndTime, Duration = s.Duration, CreatedAt = s.CreatedAt }).ToList(),
            Activities = result.Activities.Select(a => new ActivityDto { Id = a.Id, DeviceId = a.DeviceId, ApplicationId = a.ApplicationId, Url = a.Url, Timestamp = a.Timestamp, Duration = a.Duration, CreatedAt = a.CreatedAt }).ToList(),
            Thresholds = result.Thresholds.Select(t => new ThresholdDto { Id = t.Id, CategoryId = t.CategoryId, DailyLimitSec = t.DailyLimitSec, InterventionType = t.InterventionType, Active = t.Active, UpdatedAt = t.UpdatedAt, DeletedAt = t.DeletedAt }).ToList(),
            Settings = result.Settings.Select(s => new UserSettingDto { Id = s.Id, Key = s.Key, Value = s.Value, UpdatedAt = s.UpdatedAt }).ToList(),
            Categories = result.Categories.Select(c => new CategoryDto { Id = c.Id, Name = c.Name, CreatedAt = c.CreatedAt, UpdatedAt = c.UpdatedAt, DeletedAt = c.DeletedAt }).ToList(),
            ServerTime = result.ServerTime
        };
    }
}