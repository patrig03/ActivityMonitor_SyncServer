using SyncServer.Core.Domain.Entities;
using SyncServer.Core.Interfaces;

namespace SyncServer.Core.Services;

public class SyncService : ISyncService
{
    private readonly IRepository<Device> _deviceRepository;
    private readonly IRepository<Application> _applicationRepository;
    private readonly IRepository<Session> _sessionRepository;
    private readonly IRepository<Activity> _activityRepository;
    private readonly IRepository<Threshold> _thresholdRepository;
    private readonly IRepository<UserSetting> _userSettingRepository;
    private readonly IRepository<Category> _categoryRepository;

    public SyncService(
        IRepository<Device> deviceRepository,
        IRepository<Application> applicationRepository,
        IRepository<Session> sessionRepository,
        IRepository<Activity> activityRepository,
        IRepository<Threshold> thresholdRepository,
        IRepository<UserSetting> userSettingRepository,
        IRepository<Category> categoryRepository)
    {
        _deviceRepository = deviceRepository;
        _applicationRepository = applicationRepository;
        _sessionRepository = sessionRepository;
        _activityRepository = activityRepository;
        _thresholdRepository = thresholdRepository;
        _userSettingRepository = userSettingRepository;
        _categoryRepository = categoryRepository;
    }

    public Task<SyncResult> SyncAsync(Guid userId, Guid deviceId, SyncRequest request)
    {
        return Task.Run(async () =>
        {
            await SyncEntitiesAsync(userId, request.Devices, _deviceRepository);
            await SyncEntitiesAsync(userId, request.Applications, _applicationRepository);
            await SyncEntitiesAsync(userId, request.Sessions, _sessionRepository);
            await SyncEntitiesAsync(userId, request.Activities, _activityRepository);
            await SyncEntitiesAsync(userId, request.Thresholds, _thresholdRepository);
            await SyncEntitiesAsync(userId, request.Settings, _userSettingRepository);
            await SyncEntitiesAsync(userId, request.Categories, _categoryRepository);

            var serverDevices = await _deviceRepository.GetAllAsync();
            var serverApplications = await _applicationRepository.GetAllAsync();
            var serverSessions = await _sessionRepository.GetAllAsync();
            var serverActivities = await _activityRepository.GetAllAsync();
            var serverThresholds = await _thresholdRepository.GetAllAsync();
            var serverSettings = await _userSettingRepository.GetAllAsync();
            var serverCategories = await _categoryRepository.GetAllAsync();

            return new SyncResult
            {
                Devices = serverDevices.Where(d => d.UserId == userId).ToList(),
                Applications = serverApplications.Where(a => a.UserId == userId).ToList(),
                Sessions = serverSessions.Where(s => s.UserId == userId).ToList(),
                Activities = serverActivities.Where(a => a.UserId == userId).ToList(),
                Thresholds = serverThresholds.Where(t => t.UserId == userId).ToList(),
                Settings = serverSettings.Where(s => s.UserId == userId).ToList(),
                Categories = serverCategories.Where(c => c.UserId == userId).ToList(),
                ServerTime = DateTime.UtcNow
            };
        });
    }

    private async Task SyncEntitiesAsync<T>(Guid userId, List<T> entities, IRepository<T> repository) where T : class
    {
        var existing = (await repository.GetAllAsync()).ToList();
        var entityType = typeof(T);

        foreach (var entity in entities)
        {
            var idProp = entityType.GetProperty("Id");
            var id = (Guid)(idProp?.GetValue(entity) ?? Guid.Empty);

            var existingEntity = existing.FirstOrDefault(e =>
            {
                var eId = entityType.GetProperty("Id")?.GetValue(e);
                return eId?.Equals(id) ?? false;
            });

            if (existingEntity == null)
            {
                await repository.AddAsync(entity);
            }
            else
            {
                var updatedAtProp = entityType.GetProperty("UpdatedAt");
                if (updatedAtProp != null)
                {
                    var newUpdatedAt = updatedAtProp.GetValue(entity);
                    var existingUpdatedAt = updatedAtProp.GetValue(existingEntity);
                    if (newUpdatedAt != null && (existingUpdatedAt == null || ((DateTime)newUpdatedAt) > ((DateTime)existingUpdatedAt)))
                    {
                        await repository.UpdateAsync(entity);
                    }
                }
            }
        }
    }
}