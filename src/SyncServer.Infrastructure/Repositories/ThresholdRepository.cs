using SyncServer.Core.Domain.Entities;
using SyncServer.Core.Interfaces;
using SyncServer.Infrastructure.Data;

namespace SyncServer.Infrastructure.Repositories;

public class ThresholdRepository : Repository<Threshold>, IThresholdRepository
{
    public ThresholdRepository(AppDbContext context) : base(context) { }
}