using SyncServer.Core.Domain.Entities;

namespace SyncServer.Core.Interfaces;

public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(Guid id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<T> AddAsync(T entity);
    Task<T> UpdateAsync(T entity);
    Task DeleteAsync(Guid id);
}

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email);
}

public interface IDeviceRepository : IRepository<Device>
{
    Task<IEnumerable<Device>> GetByUserIdAsync(Guid userId);
    Task UpdateLastSyncAsync(Guid deviceId);
}

public interface IApplicationRepository : IRepository<Application> { }
public interface ISessionRepository : IRepository<Session> { }
public interface IActivityRepository : IRepository<Activity> { }
public interface IThresholdRepository : IRepository<Threshold> { }
public interface IUserSettingRepository : IRepository<UserSetting> { }
public interface ICategoryRepository : IRepository<Category> { }