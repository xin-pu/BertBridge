using BertBridge.Application.Contracts;
using BertBridge.Application.Dtos;
using BertBridge.Domain.Device;
using BertBridge.PluginSDK;

namespace BertBridge.Application.Services;

/// <summary>
/// ED 应用服务。
/// </summary>
public class ErrorDetectorAppService : IErrorDetectorAppService
{
    private readonly IDeviceRepository _deviceRepository;
    private readonly IDeviceAdapterFactory _adapterFactory;

    public ErrorDetectorAppService(IDeviceRepository deviceRepository, IDeviceAdapterFactory adapterFactory)
    {
        _deviceRepository = deviceRepository;
        _adapterFactory = adapterFactory;
    }

    public async Task<EdResultDto> StartEdAsync(Guid deviceId, int laneIndex, string expectedPattern, CancellationToken ct = default)
    {
        var device = await _deviceRepository.GetByIdAsync(new DeviceId(deviceId), ct)
            ?? throw new InvalidOperationException("设备不存在。");

        var connection = device.Connection
            ?? throw new InvalidOperationException("设备未配置连接信息。");
        var adapter = await _adapterFactory.GetOrConnectAsync(deviceId,
            PluginSDK.ConnectionString.Parse(connection.Value), ct);

        device.EnableErrorDetector(laneIndex);
        await adapter.ConfigureEdAsync(laneIndex, new EdConfiguration(expectedPattern), ct);
        await adapter.StartEdAsync(laneIndex, ct);
        await _deviceRepository.UpdateAsync(device, ct);

        var result = await adapter.ReadEdResultAsync(laneIndex, ct);
        return MapToDto(result);
    }

    public async Task<EdResultDto> StopEdAsync(Guid deviceId, int laneIndex, CancellationToken ct = default)
    {
        var device = await _deviceRepository.GetByIdAsync(new DeviceId(deviceId), ct)
            ?? throw new InvalidOperationException("设备不存在。");

        var connection = device.Connection
            ?? throw new InvalidOperationException("设备未配置连接信息。");
        var adapter = await _adapterFactory.GetOrConnectAsync(deviceId,
            PluginSDK.ConnectionString.Parse(connection.Value), ct);

        await adapter.StopEdAsync(laneIndex, ct);
        device.DisableErrorDetector(laneIndex);
        await _deviceRepository.UpdateAsync(device, ct);

        var result = await adapter.ReadEdResultAsync(laneIndex, ct);
        return MapToDto(result);
    }

    public async Task<EdResultDto> ReadEdResultAsync(Guid deviceId, int laneIndex, CancellationToken ct = default)
    {
        var device = await _deviceRepository.GetByIdAsync(new DeviceId(deviceId), ct)
            ?? throw new InvalidOperationException("设备不存在。");

        var connection = device.Connection
            ?? throw new InvalidOperationException("设备未配置连接信息。");
        var adapter = await _adapterFactory.GetOrConnectAsync(deviceId,
            PluginSDK.ConnectionString.Parse(connection.Value), ct);

        var result = await adapter.ReadEdResultAsync(laneIndex, ct);
        return MapToDto(result);
    }

    private static EdResultDto MapToDto(EdResult result) => new(
        result.ErrorCount,
        result.TotalCount,
        result.Ber,
        result.SnrDb,
        result.SignalDetected,
        result.CdrLocked,
        result.PllLocked,
        result.DspReady,
        result.FecLocked,
        result.AlignmentLocked,
        result.Timestamp
    );
}
