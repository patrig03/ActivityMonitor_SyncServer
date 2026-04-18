using SyncServer.Core.Domain.Entities;
using SyncServer.Core.Interfaces;
using SyncServer.Infrastructure.Data;

namespace SyncServer.Infrastructure.Repositories;

public class SessionRepository : Repository<Session>, ISessionRepository
{
    public SessionRepository(AppDbContext context) : base(context) { }
}