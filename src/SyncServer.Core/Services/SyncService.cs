using SyncServer.Core.Domain.Entities;
using SyncServer.Core.Interfaces;

namespace SyncServer.Core.Services;

public class SyncService : ISyncService
{
    private readonly IRepository<Session> _sessionRepository;
    private readonly IRepository<Activity> _activityRepository;
    private readonly IRepository<Threshold> _thresholdRepository;
    private readonly IRepository<UserSetting> _userSettingRepository;
    private readonly IRepository<Category> _categoryRepository;
    private readonly IRepository<Application> _applicationRepository;
    private readonly SyncValidationService _validationService;

    public SyncService(
        IRepository<Session> sessionRepository,
        IRepository<Activity> activityRepository,
        IRepository<Threshold> thresholdRepository,
        IRepository<UserSetting> userSettingRepository,
        IRepository<Category> categoryRepository,
        IRepository<Application> applicationRepository,
        SyncValidationService validationService)
    {
        _sessionRepository = sessionRepository;
        _activityRepository = activityRepository;
        _thresholdRepository = thresholdRepository;
        _userSettingRepository = userSettingRepository;
        _categoryRepository = categoryRepository;
        _applicationRepository = applicationRepository;
        _validationService = validationService;
    }

    public async Task<SyncResult> SyncAsync(Guid userId, Guid deviceId, SyncRequest request)
    {
        var (isValid, errors) = _validationService.ValidateSyncRequest(userId, request);
        if (!isValid)
        {
            throw new InvalidOperationException($"Validation failed: {string.Join(", ", errors)}");
        }

        if (request.Sessions.Any())
            await MergeSessionsAsync(userId, request.Sessions);

        if (request.Activities.Any())
            await MergeActivitiesAsync(userId, request.Activities);

        if (request.Thresholds.Any())
            await MergeThresholdsAsync(userId, request.Thresholds);

        if (request.Settings.Any())
            await MergeSettingsAsync(userId, request.Settings);

        if (request.Categories.Any())
            await MergeCategoriesAsync(userId, request.Categories);

        if (request.Applications.Any())
            await MergeApplicationsAsync(userId, request.Applications);

        var pullResult = await PullChangesAsync(userId, request.LastSyncAt);
        
        return new SyncResult
        {
            Sessions = pullResult.Sessions,
            Activities = pullResult.Activities,
            Thresholds = pullResult.Thresholds,
            Settings = pullResult.Settings,
            Categories = pullResult.Categories,
            Applications = pullResult.Applications,
            ServerTime = pullResult.ServerTime
        };
    }

    public async Task<SyncPullResult> PullChangesAsync(Guid userId, DateTime since)
    {
        var allSessions = await _sessionRepository.GetAllAsync();
        var allActivities = await _activityRepository.GetAllAsync();
        var allThresholds = await _thresholdRepository.GetAllAsync();
        var allSettings = await _userSettingRepository.GetAllAsync();
        var allCategories = await _categoryRepository.GetAllAsync();
        var allApplications = await _applicationRepository.GetAllAsync();

        return new SyncPullResult
        {
            Sessions = allSessions.Where(s => s.UserId == userId && s.CreatedAt > since).ToList(),
            Activities = allActivities.Where(a => a.UserId == userId && a.CreatedAt > since).ToList(),
            Thresholds = allThresholds.Where(t => t.UserId == userId && t.UpdatedAt > since).ToList(),
            Settings = allSettings.Where(s => s.UserId == userId && s.UpdatedAt > since).ToList(),
            Categories = allCategories.Where(c => c.UserId == userId && c.UpdatedAt > since).ToList(),
            Applications = allApplications.Where(a => a.UserId == userId && a.UpdatedAt > since).ToList(),
            ServerTime = DateTime.UtcNow
        };
    }

    private async Task MergeSessionsAsync(Guid userId, List<Session> clientSessions)
    {
        foreach (var session in clientSessions)
        {
            session.UserId = userId;
            session.Id = session.Id == Guid.Empty ? Guid.NewGuid() : session.Id;
            await _sessionRepository.AddAsync(session);
        }
    }

    private async Task MergeActivitiesAsync(Guid userId, List<Activity> clientActivities)
    {
        var existing = (await _activityRepository.GetAllAsync())
            .Where(a => a.UserId == userId)
            .ToList();

        foreach (var activity in clientActivities)
        {
            activity.UserId = userId;
            var exists = existing.Any(e => e.Id == activity.Id);
            if (!exists)
            {
                activity.Id = activity.Id == Guid.Empty ? Guid.NewGuid() : activity.Id;
                await _activityRepository.AddAsync(activity);
            }
        }
    }

    private async Task MergeThresholdsAsync(Guid userId, List<Threshold> clientThresholds)
    {
        var existing = (await _thresholdRepository.GetAllAsync())
            .Where(t => t.UserId == userId)
            .ToList();

        foreach (var threshold in clientThresholds)
        {
            threshold.UserId = userId;
            var existingEntity = existing.FirstOrDefault(e => e.Id == threshold.Id);

            if (existingEntity == null)
            {
                threshold.Id = threshold.Id == Guid.Empty ? Guid.NewGuid() : threshold.Id;
                await _thresholdRepository.AddAsync(threshold);
            }
            else if (threshold.UpdatedAt > existingEntity.UpdatedAt)
            {
                threshold.Id = existingEntity.Id;
                await _thresholdRepository.UpdateAsync(threshold);
            }
        }
    }

    private async Task MergeSettingsAsync(Guid userId, List<UserSetting> clientSettings)
    {
        var existing = (await _userSettingRepository.GetAllAsync())
            .Where(s => s.UserId == userId)
            .ToList();

        foreach (var setting in clientSettings)
        {
            setting.UserId = userId;
            var existingEntity = existing.FirstOrDefault(e => e.Id == setting.Id);

            if (existingEntity == null)
            {
                setting.Id = setting.Id == Guid.Empty ? Guid.NewGuid() : setting.Id;
                await _userSettingRepository.AddAsync(setting);
            }
            else if (setting.UpdatedAt > existingEntity.UpdatedAt)
            {
                setting.Id = existingEntity.Id;
                await _userSettingRepository.UpdateAsync(setting);
            }
        }
    }

    private async Task MergeCategoriesAsync(Guid userId, List<Category> clientCategories)
    {
        var existing = (await _categoryRepository.GetAllAsync())
            .Where(c => c.UserId == userId)
            .ToList();

        foreach (var category in clientCategories)
        {
            category.UserId = userId;
            var existingEntity = existing.FirstOrDefault(e => e.Id == category.Id);

            if (existingEntity == null)
            {
                category.Id = category.Id == Guid.Empty ? Guid.NewGuid() : category.Id;
                await _categoryRepository.AddAsync(category);
            }
            else if (category.UpdatedAt > existingEntity.UpdatedAt)
            {
                if (category.DeletedAt.HasValue)
                {
                    await _categoryRepository.DeleteAsync(category.Id);
                }
                else
                {
                    category.Id = existingEntity.Id;
                    await _categoryRepository.UpdateAsync(category);
                }
            }
        }
    }

    private async Task MergeApplicationsAsync(Guid userId, List<Application> clientApplications)
    {
        var existing = (await _applicationRepository.GetAllAsync())
            .Where(a => a.UserId == userId)
            .ToList();

        foreach (var app in clientApplications)
        {
            app.UserId = userId;
            var existingEntity = existing.FirstOrDefault(e => e.Id == app.Id);

            if (existingEntity == null)
            {
                app.Id = app.Id == Guid.Empty ? Guid.NewGuid() : app.Id;
                await _applicationRepository.AddAsync(app);
            }
            else if (app.UpdatedAt > existingEntity.UpdatedAt)
            {
                if (app.DeletedAt.HasValue)
                {
                    await _applicationRepository.DeleteAsync(app.Id);
                }
                else
                {
                    app.Id = existingEntity.Id;
                    await _applicationRepository.UpdateAsync(app);
                }
            }
        }
    }
}