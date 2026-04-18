using SyncServer.Core.Domain.Entities;
using SyncServer.Core.Interfaces;
using SyncServer.Infrastructure.Data;

namespace SyncServer.Infrastructure.Repositories;

public class ApplicationRepository : Repository<Application>, IApplicationRepository
{
    public ApplicationRepository(AppDbContext context) : base(context) { }
}