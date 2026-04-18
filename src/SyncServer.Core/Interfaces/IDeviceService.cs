using SyncServer.Core.Domain.Entities;

namespace SyncServer.Core.Interfaces;

public interface IDeviceService
{
    Task<Device?> GetByIdAsync(Guid id);
    Task<Device> CreateAsync(Guid userId, string name);
    Task UpdateLastSyncAsync(Guid deviceId);
    Task<IEnumerable<Device>> GetByUserIdAsync(Guid userId);
}