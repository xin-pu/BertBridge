using BertBridge.Application.Dtos;

namespace BertBridge.Application.Contracts;

/// <summary>
/// 设备应用服务接口。
/// </summary>
public interface IDeviceAppService
{
    Task<IReadOnlyList<DeviceListItemDto>> GetAllDevicesAsync(CancellationToken ct = default);
    Task<DeviceDto?> GetDeviceAsync(Guid deviceId, CancellationToken ct = default);
    Task<DeviceDto> ConnectAsync(string connectionString, string deviceName, CancellationToken ct = default);
    Task DisconnectAsync(Guid deviceId, CancellationToken ct = default);
    Task<IReadOnlyList<LaneDto>> GetLanesAsync(Guid deviceId, CancellationToken ct = default);
}
