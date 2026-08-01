using BertBridge.Application.Contracts;
using BertBridge.Application.Dtos;
using BertBridge.Domain.Device;
using BertBridge.PluginSDK;

namespace BertBridge.Application.Services;

/// <summary>
/// 设备应用服务。协调设备 CRUD 和连接管理。
/// </summary>
public class DeviceAppService : IDeviceAppService
{
    private readonly IDeviceRepository _deviceRepository;
    private readonly IDeviceAdapterFactory _adapterFactory;

    public DeviceAppService(IDeviceRepository deviceRepository, IDeviceAdapterFactory adapterFactory)
    {
        _deviceRepository = deviceRepository;
        _adapterFactory = adapterFactory;
    }

    public async Task<IReadOnlyList<DeviceListItemDto>> GetAllDevicesAsync(CancellationToken ct = default)
    {
        var devices = await _deviceRepository.GetAllAsync(ct);
        return devices.Select(d => new DeviceListItemDto(
            d.Id,
            d.DeviceName,
            d.Info?.Model,
            d.State.ToString()
        )).ToList();
    }

    public async Task<DeviceDto?> GetDeviceAsync(Guid deviceId, CancellationToken ct = default)
    {
        var device = await _deviceRepository.GetByIdAsync(new DeviceId(deviceId), ct);
        if (device == null) return null;

        return new DeviceDto(
            device.Id,
            device.DeviceName,
            device.Info?.Model,
            device.Info?.SerialNumber,
            device.Info?.FirmwareVersion,
            device.Connection?.Value,
            device.State.ToString(),
            device.Lanes.Count
        );
    }

    public async Task<DeviceDto> ConnectAsync(string connectionString, string deviceName, CancellationToken ct = default)
    {
        var cs = PluginSDK.ConnectionString.Parse(connectionString);

        // 1. 创建或获取设备聚合
        var existing = await _deviceRepository.GetByConnectionStringAsync(
            Domain.Device.ConnectionString.Parse(connectionString), ct);

        var device = existing ?? Domain.Device.Device.Create(deviceName);

        // 2. 开始连接
        device.BeginConnect(Domain.Device.ConnectionString.Parse(connectionString));

        // 3. 获取适配器并连接
        var adapter = _adapterFactory.GetAdapter(device.Id)
            ?? throw new InvalidOperationException($"没有适配器可以处理连接: {connectionString}");

        await adapter.ConnectAsync(cs, ct);

        // 4. 注册设备信息
        var info = await adapter.GetDeviceInfoAsync(ct);
        var domainInfo = new Domain.Device.DeviceInfo(info.Model, info.SerialNumber, info.FirmwareVersion, info.BoardType);
        var cap = adapter.Capability;
        var domainCap = new Domain.Device.DeviceCapability(
            cap.MaxLanes, cap.SupportsPAM4, cap.SupportsAdvancedModulation,
            cap.SupportedPatterns, cap.MaxBaudRateGBd, cap.SupportsFec,
            cap.SupportsGpio, cap.FirTapCount, cap.SupportsJitterInjection);

        device.RegisterDeviceInfo(domainInfo, domainCap);
        device.MarkConnected(Domain.Device.ConnectionString.Parse(connectionString));

        // 5. 持久化
        if (existing == null)
            await _deviceRepository.AddAsync(device, ct);
        else
            await _deviceRepository.UpdateAsync(device, ct);

        return new DeviceDto(device.Id, device.DeviceName, info.Model,
            info.SerialNumber, info.FirmwareVersion, connectionString,
            "Connected", device.Lanes.Count);
    }

    public async Task DisconnectAsync(Guid deviceId, CancellationToken ct = default)
    {
        var device = await _deviceRepository.GetByIdAsync(new DeviceId(deviceId), ct)
            ?? throw new InvalidOperationException("设备不存在。");

        var adapter = _adapterFactory.GetAdapter(deviceId);
        if (adapter != null)
        {
            await adapter.DisconnectAsync();
            _adapterFactory.UnregisterAdapter(deviceId);
        }

        device.Disconnect();
        await _deviceRepository.UpdateAsync(device, ct);
    }

    public async Task<IReadOnlyList<LaneDto>> GetLanesAsync(Guid deviceId, CancellationToken ct = default)
    {
        var device = await _deviceRepository.GetByIdAsync(new DeviceId(deviceId), ct)
            ?? throw new InvalidOperationException("设备不存在。");

        return device.Lanes.Select(l => new LaneDto(
            l.Id, l.LaneIndex, l.LaneName,
            l.PgEnabled, l.EdEnabled, l.CurrentPattern
        )).ToList();
    }
}
