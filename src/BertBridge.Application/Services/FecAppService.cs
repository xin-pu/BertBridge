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
    private readonly IDeviceAdapterFactory _adapterFactory;

    public FecAppService(IDeviceAdapterFactory adapterFactory)
    {
        _adapterFactory = adapterFactory;
    }

    public async Task<FecStatisticsDto> ReadFecStatisticsAsync(Guid deviceId, int chipIndex, CancellationToken ct = default)
    {
        var adapter = _adapterFactory.GetAdapter(deviceId)
            ?? throw new InvalidOperationException("设备未连接。");

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
