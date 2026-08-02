using BertBridge.Application.Contracts;
using BertBridge.Application.Dtos;
using BertBridge.Domain.Device;
using BertBridge.PluginSDK;

namespace BertBridge.Application.Services;

/// <summary>
/// FEC 应用服务。
/// </summary>
public class FecAppService : IFecAppService
{
    private readonly IDeviceRepository _deviceRepository;
    private readonly IDeviceAdapterFactory _adapterFactory;

    public FecAppService(IDeviceRepository deviceRepository, IDeviceAdapterFactory adapterFactory)
    {
        _deviceRepository = deviceRepository;
        _adapterFactory = adapterFactory;
    }

    public async Task<FecStatisticsDto> ReadFecStatisticsAsync(Guid deviceId, int chipIndex, CancellationToken ct = default)
    {
        var device = await _deviceRepository.GetByIdAsync(new DeviceId(deviceId), ct)
            ?? throw new InvalidOperationException("设备不存在。");

        var connection = device.Connection
            ?? throw new InvalidOperationException("设备未配置连接信息。");
        var adapter = await _adapterFactory.GetOrConnectAsync(deviceId,
            PluginSDK.ConnectionString.Parse(connection.Value), ct);

        var stats = await adapter.ReadFecStatisticsAsync(chipIndex, ct);
        return new FecStatisticsDto(
            stats.PreFecBer,
            stats.PostFecBer,
            stats.CorrectableCodewords,
            stats.UncorrectableCodewords,
            stats.SymbolErrors,
            stats.IsLocked,
            stats.Timestamp
        );
    }
}
