namespace SyncServer.Api.DTOs;

public class CreateDeviceRequest
{
    public string Name { get; set; } = string.Empty;
}

public class DeviceResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string DeviceType { get; set; } = "Desktop";
    public string Platform { get; set; } = string.Empty;
    public string? Fingerprint { get; set; }
    public string? AppVersion { get; set; }
    public string Status { get; set; } = "Active";
    public bool IsTrusted { get; set; }
    public bool IsCurrentDevice { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime LastSeenAt { get; set; }
    public DateTime LastSyncAt { get; set; }
    public DateTime? RevokedAt { get; set; }
}

public class DeviceListResponse
{
    public List<DeviceResponse> Devices { get; set; } = new();
}