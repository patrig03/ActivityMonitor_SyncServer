using SyncServer.Core.Domain.Entities;
using SyncServer.Core.Interfaces;
using SyncServer.Infrastructure.Data;

namespace SyncServer.Infrastructure.Repositories;

public class UserSettingRepository : Repository<UserSetting>, IUserSettingRepository
{
    public UserSettingRepository(AppDbContext context) : base(context) { }
}