namespace SyncServer.Api.DTOs;

public class CreateDeviceRequest
{
    public string Name { get; set; } = string.Empty;
}

public class DeviceResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime LastSyncAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class DeviceListResponse
{
    public List<DeviceResponse> Devices { get; set; } = new();
}