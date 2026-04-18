using SyncServer.Core.Domain.Entities;
using SyncServer.Core.Interfaces;
using SyncServer.Infrastructure.Data;

namespace SyncServer.Infrastructure.Repositories;

public class ActivityRepository : Repository<Activity>, IActivityRepository
{
    public ActivityRepository(AppDbContext context) : base(context) { }
}