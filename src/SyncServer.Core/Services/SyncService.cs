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
        if (request.DeviceId == Guid.Empty)
        {
            request.DeviceId = deviceId;
        }

        var (isValid, errors) = _validationService.ValidateSyncRequest(userId, request);
        if (!isValid)
        {
            throw new InvalidOperationException($"Validation failed: {string.Join(", ", errors)}");
        }

        var syncTimestamp = DateTime.UtcNow;

        if (request.Sessions.Any())
        {
            await MergeSessionsAsync(userId, request.Sessions, syncTimestamp);
        }

        if (request.Activities.Any())
        {
            await MergeActivitiesAsync(userId, request.Activities, syncTimestamp);
        }

        if (request.Thresholds.Any())
        {
            await MergeThresholdsAsync(userId, request.Thresholds, syncTimestamp);
        }

        if (request.Settings.Any())
        {
            await MergeSettingsAsync(userId, request.Settings, syncTimestamp);
        }

        if (request.Categories.Any())
        {
            await MergeCategoriesAsync(userId, request.Categories, syncTimestamp);
        }

        if (request.Applications.Any())
        {
            await MergeApplicationsAsync(userId, request.Applications, syncTimestamp);
        }

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
        var normalizedSince = since == default ? DateTime.UnixEpoch : since;
        var allSessions = await _sessionRepository.GetAllAsync();
        var allActivities = await _activityRepository.GetAllAsync();
        var allThresholds = await _thresholdRepository.GetAllAsync();
        var allSettings = await _userSettingRepository.GetAllAsync();
        var allCategories = await _categoryRepository.GetAllAsync();
        var allApplications = await _applicationRepository.GetAllAsync();

        return new SyncPullResult
        {
            Sessions = allSessions.Where(s => s.UserId == userId && s.CreatedAt > normalizedSince).ToList(),
            Activities = allActivities.Where(a => a.UserId == userId && a.CreatedAt > normalizedSince).ToList(),
            Thresholds = allThresholds.Where(t => t.UserId == userId && t.UpdatedAt > normalizedSince).ToList(),
            Settings = allSettings.Where(s => s.UserId == userId && s.UpdatedAt > normalizedSince).ToList(),
            Categories = allCategories.Where(c => c.UserId == userId && c.UpdatedAt > normalizedSince).ToList(),
            Applications = allApplications.Where(a => a.UserId == userId && a.UpdatedAt > normalizedSince).ToList(),
            ServerTime = DateTime.UtcNow
        };
    }

    private async Task MergeSessionsAsync(Guid userId, IEnumerable<Session> clientSessions, DateTime syncTimestamp)
    {
        var existing = (await _sessionRepository.GetAllAsync())
            .Where(s => s.UserId == userId)
            .ToDictionary(s => s.Id);

        foreach (var session in clientSessions)
        {
            session.UserId = userId;
            session.Id = session.Id == Guid.Empty ? Guid.NewGuid() : session.Id;

            if (!existing.TryGetValue(session.Id, out var existingEntity))
            {
                session.CreatedAt = syncTimestamp;
                await _sessionRepository.AddAsync(session);
                existing[session.Id] = session;
                continue;
            }

            if (!ShouldUpdateSession(existingEntity, session))
            {
                continue;
            }

            existingEntity.DeviceId = session.DeviceId;
            existingEntity.ApplicationId = session.ApplicationId;
            existingEntity.StartTime = session.StartTime;
            existingEntity.EndTime = session.EndTime;
            existingEntity.Duration = session.Duration;
            existingEntity.CreatedAt = syncTimestamp;
            await _sessionRepository.UpdateAsync(existingEntity);
        }
    }

    private async Task MergeActivitiesAsync(Guid userId, IEnumerable<Activity> clientActivities, DateTime syncTimestamp)
    {
        var existing = (await _activityRepository.GetAllAsync())
            .Where(a => a.UserId == userId)
            .ToDictionary(a => a.Id);

        foreach (var activity in clientActivities)
        {
            activity.UserId = userId;
            activity.Id = activity.Id == Guid.Empty ? Guid.NewGuid() : activity.Id;

            if (!existing.TryGetValue(activity.Id, out var existingEntity))
            {
                activity.CreatedAt = syncTimestamp;
                await _activityRepository.AddAsync(activity);
                existing[activity.Id] = activity;
                continue;
            }

            if (!ShouldUpdateActivity(existingEntity, activity))
            {
                continue;
            }

            existingEntity.DeviceId = activity.DeviceId;
            existingEntity.ApplicationId = activity.ApplicationId;
            existingEntity.CategoryId = activity.CategoryId;
            existingEntity.Url = activity.Url;
            existingEntity.Timestamp = activity.Timestamp;
            existingEntity.Duration = activity.Duration;
            existingEntity.CreatedAt = syncTimestamp;
            await _activityRepository.UpdateAsync(existingEntity);
        }
    }

    private async Task MergeThresholdsAsync(Guid userId, IEnumerable<Threshold> clientThresholds, DateTime syncTimestamp)
    {
        var existing = (await _thresholdRepository.GetAllAsync())
            .Where(t => t.UserId == userId)
            .ToDictionary(t => t.Id);

        foreach (var threshold in clientThresholds)
        {
            threshold.UserId = userId;
            threshold.Id = threshold.Id == Guid.Empty ? Guid.NewGuid() : threshold.Id;

            if (!existing.TryGetValue(threshold.Id, out var existingEntity))
            {
                threshold.UpdatedAt = syncTimestamp;
                await _thresholdRepository.AddAsync(threshold);
                existing[threshold.Id] = threshold;
                continue;
            }

            if (!ShouldUpdateThreshold(existingEntity, threshold))
            {
                continue;
            }

            existingEntity.CategoryId = threshold.CategoryId;
            existingEntity.ApplicationId = threshold.ApplicationId;
            existingEntity.Active = threshold.Active;
            existingEntity.TargetType = threshold.TargetType;
            existingEntity.InterventionType = threshold.InterventionType;
            existingEntity.DurationType = threshold.DurationType;
            existingEntity.SessionLimitSec = threshold.SessionLimitSec;
            existingEntity.DailyLimitSec = threshold.DailyLimitSec;
            existingEntity.DeletedAt = threshold.DeletedAt;
            existingEntity.UpdatedAt = syncTimestamp;
            await _thresholdRepository.UpdateAsync(existingEntity);
        }
    }

    private async Task MergeSettingsAsync(Guid userId, IEnumerable<UserSetting> clientSettings, DateTime syncTimestamp)
    {
        var existing = (await _userSettingRepository.GetAllAsync())
            .Where(s => s.UserId == userId)
            .ToList();
        var existingById = existing.ToDictionary(s => s.Id);
        var existingByKey = existing
            .Where(s => !string.IsNullOrWhiteSpace(s.Key))
            .ToDictionary(s => s.Key, StringComparer.Ordinal);

        foreach (var setting in clientSettings)
        {
            setting.UserId = userId;
            setting.Id = setting.Id == Guid.Empty ? Guid.NewGuid() : setting.Id;

            if (!existingById.TryGetValue(setting.Id, out var existingEntity) &&
                !string.IsNullOrWhiteSpace(setting.Key))
            {
                existingByKey.TryGetValue(setting.Key, out existingEntity);
            }

            if (existingEntity == null)
            {
                setting.UpdatedAt = syncTimestamp;
                await _userSettingRepository.AddAsync(setting);
                existingById[setting.Id] = setting;
                if (!string.IsNullOrWhiteSpace(setting.Key))
                {
                    existingByKey[setting.Key] = setting;
                }
                continue;
            }

            if (!ShouldUpdateSetting(existingEntity, setting))
            {
                continue;
            }

            existingEntity.Key = setting.Key;
            existingEntity.Value = setting.Value;
            existingEntity.UpdatedAt = syncTimestamp;
            await _userSettingRepository.UpdateAsync(existingEntity);
            existingById[existingEntity.Id] = existingEntity;
            if (!string.IsNullOrWhiteSpace(existingEntity.Key))
            {
                existingByKey[existingEntity.Key] = existingEntity;
            }
        }
    }

    private async Task MergeCategoriesAsync(Guid userId, IEnumerable<Category> clientCategories, DateTime syncTimestamp)
    {
        var existing = (await _categoryRepository.GetAllAsync())
            .Where(c => c.UserId == userId)
            .ToDictionary(c => c.Id);

        foreach (var category in clientCategories)
        {
            category.UserId = userId;
            category.Id = category.Id == Guid.Empty ? Guid.NewGuid() : category.Id;

            if (!existing.TryGetValue(category.Id, out var existingEntity))
            {
                category.CreatedAt = syncTimestamp;
                category.UpdatedAt = syncTimestamp;
                await _categoryRepository.AddAsync(category);
                existing[category.Id] = category;
                continue;
            }

            if (!ShouldUpdateCategory(existingEntity, category))
            {
                continue;
            }

            existingEntity.Name = category.Name;
            existingEntity.Description = category.Description;
            existingEntity.DeletedAt = category.DeletedAt;
            existingEntity.UpdatedAt = syncTimestamp;
            await _categoryRepository.UpdateAsync(existingEntity);
        }
    }

    private async Task MergeApplicationsAsync(Guid userId, IEnumerable<Application> clientApplications, DateTime syncTimestamp)
    {
        var existing = (await _applicationRepository.GetAllAsync())
            .Where(a => a.UserId == userId)
            .ToDictionary(a => a.Id);

        foreach (var app in clientApplications)
        {
            app.UserId = userId;
            app.Id = app.Id == Guid.Empty ? Guid.NewGuid() : app.Id;

            if (!existing.TryGetValue(app.Id, out var existingEntity))
            {
                app.CreatedAt = syncTimestamp;
                app.UpdatedAt = syncTimestamp;
                await _applicationRepository.AddAsync(app);
                existing[app.Id] = app;
                continue;
            }

            if (!ShouldUpdateApplication(existingEntity, app))
            {
                continue;
            }

            existingEntity.Name = app.Name;
            existingEntity.CategoryId = app.CategoryId;
            existingEntity.WindowTitle = app.WindowTitle;
            existingEntity.ClassName = app.ClassName;
            existingEntity.ProcessName = app.ProcessName;
            existingEntity.PositionX = app.PositionX;
            existingEntity.PositionY = app.PositionY;
            existingEntity.Width = app.Width;
            existingEntity.Height = app.Height;
            existingEntity.WindowId = app.WindowId;
            existingEntity.DeletedAt = app.DeletedAt;
            existingEntity.UpdatedAt = syncTimestamp;
            await _applicationRepository.UpdateAsync(existingEntity);
        }
    }

    private static bool ShouldUpdateSession(Session existing, Session incoming)
    {
        return existing.DeviceId != incoming.DeviceId ||
               existing.ApplicationId != incoming.ApplicationId ||
               existing.StartTime != incoming.StartTime ||
               existing.EndTime != incoming.EndTime ||
               existing.Duration != incoming.Duration;
    }

    private static bool ShouldUpdateActivity(Activity existing, Activity incoming)
    {
        return existing.DeviceId != incoming.DeviceId ||
               existing.ApplicationId != incoming.ApplicationId ||
               existing.CategoryId != incoming.CategoryId ||
               !string.Equals(existing.Url, incoming.Url, StringComparison.Ordinal) ||
               existing.Timestamp != incoming.Timestamp ||
               existing.Duration != incoming.Duration;
    }

    private static bool ShouldUpdateThreshold(Threshold existing, Threshold incoming)
    {
        return existing.CategoryId != incoming.CategoryId ||
               existing.ApplicationId != incoming.ApplicationId ||
               existing.Active != incoming.Active ||
               !string.Equals(existing.TargetType, incoming.TargetType, StringComparison.Ordinal) ||
               !string.Equals(existing.InterventionType, incoming.InterventionType, StringComparison.Ordinal) ||
               !string.Equals(existing.DurationType, incoming.DurationType, StringComparison.Ordinal) ||
               existing.SessionLimitSec != incoming.SessionLimitSec ||
               existing.DailyLimitSec != incoming.DailyLimitSec ||
               existing.DeletedAt != incoming.DeletedAt;
    }

    private static bool ShouldUpdateSetting(UserSetting existing, UserSetting incoming)
    {
        return !string.Equals(existing.Key, incoming.Key, StringComparison.Ordinal) ||
               !string.Equals(existing.Value, incoming.Value, StringComparison.Ordinal);
    }

    private static bool ShouldUpdateCategory(Category existing, Category incoming)
    {
        return !string.Equals(existing.Name, incoming.Name, StringComparison.Ordinal) ||
               !string.Equals(existing.Description, incoming.Description, StringComparison.Ordinal) ||
               existing.DeletedAt != incoming.DeletedAt;
    }

    private static bool ShouldUpdateApplication(Application existing, Application incoming)
    {
        return !string.Equals(existing.Name, incoming.Name, StringComparison.Ordinal) ||
               existing.CategoryId != incoming.CategoryId ||
               !string.Equals(existing.WindowTitle, incoming.WindowTitle, StringComparison.Ordinal) ||
               !string.Equals(existing.ClassName, incoming.ClassName, StringComparison.Ordinal) ||
               !string.Equals(existing.ProcessName, incoming.ProcessName, StringComparison.Ordinal) ||
               existing.PositionX != incoming.PositionX ||
               existing.PositionY != incoming.PositionY ||
               existing.Width != incoming.Width ||
               existing.Height != incoming.Height ||
               existing.WindowId != incoming.WindowId ||
               existing.DeletedAt != incoming.DeletedAt;
    }
}
