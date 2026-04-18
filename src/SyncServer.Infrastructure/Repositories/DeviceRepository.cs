using Microsoft.EntityFrameworkCore;
using SyncServer.Core.Domain.Entities;
using SyncServer.Core.Interfaces;
using SyncServer.Infrastructure.Data;

namespace SyncServer.Infrastructure.Repositories;

public class DeviceRepository : Repository<Device>, IDeviceRepository
{
    public DeviceRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<Device>> GetByUserIdAsync(Guid userId)
    {
        return await _dbSet.Where(d => d.UserId == userId).ToListAsync();
    }

    public async Task UpdateLastSyncAsync(Guid deviceId)
    {
        var device = await _dbSet.FindAsync(deviceId);
        if (device != null)
        {
            device.LastSyncAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }
}