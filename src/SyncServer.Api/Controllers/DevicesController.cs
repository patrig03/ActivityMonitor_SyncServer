using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SyncServer.Api.DTOs;
using SyncServer.Core.Interfaces;

namespace SyncServer.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DevicesController : ControllerBase
{
    private readonly IDeviceService _deviceService;

    public DevicesController(IDeviceService deviceService)
    {
        _deviceService = deviceService;
    }

    [HttpGet]
    public async Task<ActionResult<DeviceListResponse>> GetAll()
    {
        var userId = GetUserId();
        var devices = await _deviceService.GetByUserIdAsync(userId);

        return Ok(new DeviceListResponse
        {
            Devices = devices.Select(d => new DeviceResponse
            {
                Id = d.Id,
                Name = d.Name,
                DeviceType = d.DeviceType,
                Platform = d.Platform,
                Fingerprint = d.Fingerprint,
                AppVersion = d.AppVersion,
                Status = d.Status,
                IsTrusted = d.IsTrusted,
                IsCurrentDevice = d.IsCurrentDevice,
                CreatedAt = d.CreatedAt,
                LastSeenAt = d.LastSeenAt,
                LastSyncAt = d.LastSyncAt,
                RevokedAt = d.RevokedAt
            }).ToList()
        });
    }

    [HttpPost]
    public async Task<ActionResult<DeviceResponse>> Create([FromBody] CreateDeviceRequest request)
    {
        var userId = GetUserId();
        var device = await _deviceService.CreateAsync(userId, request.Name);

        return Ok(new DeviceResponse
        {
            Id = device.Id,
            Name = device.Name,
            DeviceType = device.DeviceType,
            Platform = device.Platform,
            Status = device.Status,
            IsTrusted = device.IsTrusted,
            IsCurrentDevice = device.IsCurrentDevice,
            CreatedAt = device.CreatedAt,
            LastSeenAt = device.LastSeenAt,
            LastSyncAt = device.LastSyncAt,
            RevokedAt = device.RevokedAt
        });
    }

    private Guid GetUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(claim!);
    }
}