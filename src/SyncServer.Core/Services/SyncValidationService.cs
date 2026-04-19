using SyncServer.Core.Domain.Entities;
using SyncServer.Core.Interfaces;

namespace SyncServer.Core.Services;

public class SyncValidationService
{
    public (bool IsValid, List<string> Errors) ValidateSyncRequest(Guid userId, SyncRequest request)
    {
        var errors = new List<string>();

        foreach (var session in request.Sessions)
        {
            var sessionErrors = ValidateSession(userId, session);
            errors.AddRange(sessionErrors);
        }

        foreach (var activity in request.Activities)
        {
            var activityErrors = ValidateActivity(userId, activity);
            errors.AddRange(activityErrors);
        }

        foreach (var threshold in request.Thresholds)
        {
            var thresholdErrors = ValidateThreshold(userId, threshold);
            errors.AddRange(thresholdErrors);
        }

        foreach (var setting in request.Settings)
        {
            var settingErrors = ValidateUserSetting(userId, setting);
            errors.AddRange(settingErrors);
        }

        foreach (var category in request.Categories)
        {
            var categoryErrors = ValidateCategory(userId, category);
            errors.AddRange(categoryErrors);
        }

        foreach (var app in request.Applications)
        {
            var appErrors = ValidateApplication(userId, app);
            errors.AddRange(appErrors);
        }

        return (errors.Count == 0, errors);
    }

    private List<string> ValidateSession(Guid userId, Session session)
    {
        var errors = new List<string>();

        if (session.DeviceId == Guid.Empty)
            errors.Add($"Session {session.Id}: DeviceId is required");

        if (session.ApplicationId == Guid.Empty)
            errors.Add($"Session {session.Id}: ApplicationId is required");

        if (session.StartTime == default)
            errors.Add($"Session {session.Id}: StartTime is required");

        if (session.StartTime > DateTime.UtcNow.AddHours(1))
            errors.Add($"Session {session.Id}: StartTime cannot be in the future");

        if (session.CreatedAt > DateTime.UtcNow.AddHours(1))
            errors.Add($"Session {session.Id}: CreatedAt cannot be in the future");

        return errors;
    }

    private List<string> ValidateActivity(Guid userId, Activity activity)
    {
        var errors = new List<string>();

        if (activity.DeviceId == Guid.Empty)
            errors.Add($"Activity {activity.Id}: DeviceId is required");

        if (activity.ApplicationId == Guid.Empty)
            errors.Add($"Activity {activity.Id}: ApplicationId is required");

        if (activity.Timestamp == default)
            errors.Add($"Activity {activity.Id}: Timestamp is required");

        if (activity.Timestamp > DateTime.UtcNow.AddHours(1))
            errors.Add($"Activity {activity.Id}: Timestamp cannot be in the future");

        if (!string.IsNullOrEmpty(activity.Url) && activity.Url.Length > 2048)
            errors.Add($"Activity {activity.Id}: Url exceeds maximum length of 2048");

        return errors;
    }

    private List<string> ValidateThreshold(Guid userId, Threshold threshold)
    {
        var errors = new List<string>();

        if (threshold.DailyLimitSec < 0)
            errors.Add($"Threshold {threshold.Id}: DailyLimitSec must be >= 0");

        if (string.IsNullOrWhiteSpace(threshold.InterventionType))
            errors.Add($"Threshold {threshold.Id}: InterventionType is required");

        if (threshold.InterventionType?.Length > 50)
            errors.Add($"Threshold {threshold.Id}: InterventionType exceeds maximum length of 50");

        return errors;
    }

    private List<string> ValidateUserSetting(Guid userId, UserSetting setting)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(setting.Key))
            errors.Add($"Setting {setting.Id}: Key is required");

        if (setting.Key?.Length > 255)
            errors.Add($"Setting {setting.Id}: Key exceeds maximum length of 255");

        if (setting.Value?.Length > 4096)
            errors.Add($"Setting {setting.Id}: Value exceeds maximum length of 4096");

        return errors;
    }

    private List<string> ValidateCategory(Guid userId, Category category)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(category.Name))
            errors.Add($"Category {category.Id}: Name is required");

        if (category.Name?.Length > 255)
            errors.Add($"Category {category.Id}: Name exceeds maximum length of 255");

        return errors;
    }

    private List<string> ValidateApplication(Guid userId, Application app)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(app.Name))
            errors.Add($"Application {app.Id}: Name is required");

        if (app.Name?.Length > 255)
            errors.Add($"Application {app.Id}: Name exceeds maximum length of 255");

        return errors;
    }
}