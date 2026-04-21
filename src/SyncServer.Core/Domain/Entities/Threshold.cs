namespace SyncServer.Core.Domain.Entities;

public class Threshold
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid? CategoryId { get; set; }
    public Guid? ApplicationId { get; set; }
    public string TargetType { get; set; } = "Category";
    public int DailyLimitSec { get; set; }
    public string DurationType { get; set; } = "Daily";
    public int SessionLimitSec { get; set; }
    public string InterventionType { get; set; } = string.Empty;
    public bool Active { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}
