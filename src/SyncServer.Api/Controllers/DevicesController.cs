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
                LastSyncAt = d.LastSyncAt,
                CreatedAt = d.CreatedAt
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
            LastSyncAt = device.LastSyncAt,
            CreatedAt = device.CreatedAt
        });
    }

    private Guid GetUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(claim!);
    }
}