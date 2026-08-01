using BertBridge.Application.Contracts;
using BertBridge.Application.Dtos;
using BertBridge.Domain.Device;
using BertBridge.PluginSDK;

namespace BertBridge.Application.Services;

/// <summary>
/// PG 应用服务。
/// </summary>
public class PatternGeneratorAppService : IPatternGeneratorAppService
{
    private readonly IDeviceRepository _deviceRepository;
    private readonly IDeviceAdapterFactory _adapterFactory;

    public PatternGeneratorAppService(IDeviceRepository deviceRepository, IDeviceAdapterFactory adapterFactory)
    {
        _deviceRepository = deviceRepository;
        _adapterFactory = adapterFactory;
    }

    public async Task ConfigurePgAsync(Guid deviceId, int laneIndex, PgConfigurationDto config, CancellationToken ct = default)
    {
        var device = await _deviceRepository.GetByIdAsync(new DeviceId(deviceId), ct)
            ?? throw new InvalidOperationException("设备不存在。");

        var adapter = _adapterFactory.GetAdapter(deviceId)
            ?? throw new InvalidOperationException("设备未连接。");

        await adapter.ConfigurePgAsync(laneIndex, new PgConfiguration(
            config.Pattern, config.Mode, config.CustomPattern,
            config.MsbPattern, config.LsbPattern, config.FirTaps,
            config.SwingMillivolts, config.GrayEncoding,
            config.PolarityInvert, config.PreCoding
        ), ct);
    }

    public async Task EnablePgAsync(Guid deviceId, int laneIndex, CancellationToken ct = default)
    {
        var device = await _deviceRepository.GetByIdAsync(new DeviceId(deviceId), ct)
            ?? throw new InvalidOperationException("设备不存在。");

        var adapter = _adapterFactory.GetAdapter(deviceId)
            ?? throw new InvalidOperationException("设备未连接。");

        var lane = device.GetLane(laneIndex);
        device.EnablePatternGenerator(laneIndex, lane.CurrentPattern ?? "PRBS31");
        await adapter.EnablePgAsync(laneIndex, true, ct);
        await _deviceRepository.UpdateAsync(device, ct);
    }

    public async Task DisablePgAsync(Guid deviceId, int laneIndex, CancellationToken ct = default)
    {
        var device = await _deviceRepository.GetByIdAsync(new DeviceId(deviceId), ct)
            ?? throw new InvalidOperationException("设备不存在。");

        var adapter = _adapterFactory.GetAdapter(deviceId)
            ?? throw new InvalidOperationException("设备未连接。");

        device.DisablePatternGenerator(laneIndex);
        await adapter.EnablePgAsync(laneIndex, false, ct);
        await _deviceRepository.UpdateAsync(device, ct);
    }
}
