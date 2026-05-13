using SyncServer.Core.Domain.Entities;
using SyncServer.Core.Interfaces;

namespace SyncServer.Core.Services;

public class DeviceService : IDeviceService
{
    private readonly IRepository<Device> _deviceRepository;

    public DeviceService(IRepository<Device> deviceRepository)
    {
        _deviceRepository = deviceRepository;
    }

    public async Task<Device?> GetByIdAsync(Guid id)
    {
        return await _deviceRepository.GetByIdAsync(id);
    }

    public async Task<Device> CreateAsync(Guid userId, string name)
    {
        var device = new Device
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = name,
            DeviceType = "Desktop",
            Platform = string.Empty,
            Status = "Active",
            IsTrusted = false,
            IsCurrentDevice = false,
            LastSeenAt = DateTime.UtcNow,
            LastSyncAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        return await _deviceRepository.AddAsync(device);
    }

    public async Task UpdateLastSyncAsync(Guid deviceId)
    {
        var device = await _deviceRepository.GetByIdAsync(deviceId);
        if (device != null)
        {
            device.LastSyncAt = DateTime.UtcNow;
            await _deviceRepository.UpdateAsync(device);
        }
    }

    public async Task<IEnumerable<Device>> GetByUserIdAsync(Guid userId)
    {
        var devices = await _deviceRepository.GetAllAsync();
        return devices.Where(d => d.UserId == userId);
    }
}