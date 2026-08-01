using BertBridge.Application.Contracts;
using BertBridge.Application.Dtos;
using BertBridge.Domain.Device;
using BertBridge.PluginSDK;

namespace BertBridge.Application.Services;

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
        if (device == null)
            return null;

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
        var sdkConnection = PluginSDK.ConnectionString.Parse(connectionString);
        var domainConnection = Domain.Device.ConnectionString.Parse(connectionString);
        var adapter = _adapterFactory.CreateAdapter(sdkConnection);

        var existing = await _deviceRepository.GetByConnectionStringAsync(domainConnection, ct);
        var device = existing ?? Domain.Device.Device.Create(deviceName);

        if (existing != null)
        {
            device.Rename(deviceName);
            device.Disconnect();
        }

        device.BeginConnect(domainConnection);

        try
        {
            await adapter.ConnectAsync(sdkConnection, ct);
        }
        catch (Exception ex)
        {
            device.MarkError(ex.Message);
            await adapter.DisposeAsync();
            throw;
        }

        var info = await adapter.GetDeviceInfoAsync(ct);
        var domainInfo = new Domain.Device.DeviceInfo(
            info.Model,
            info.SerialNumber,
            info.FirmwareVersion,
            info.BoardType);

        var cap = adapter.Capability;
        var domainCap = new Domain.Device.DeviceCapability(
            cap.MaxLanes,
            cap.SupportsPAM4,
            cap.SupportsAdvancedModulation,
            cap.SupportedPatterns,
            cap.MaxBaudRateGBd,
            cap.SupportsFec,
            cap.SupportsGpio,
            cap.FirTapCount,
            cap.SupportsJitterInjection);

        device.RegisterDeviceInfo(domainInfo, domainCap);
        device.MarkConnected(domainConnection);
        _adapterFactory.RegisterAdapter(device.Id, adapter);

        if (existing == null)
            await _deviceRepository.AddAsync(device, ct);
        else
            await _deviceRepository.UpdateAsync(device, ct);

        return new DeviceDto(
            device.Id,
            device.DeviceName,
            info.Model,
            info.SerialNumber,
            info.FirmwareVersion,
            connectionString,
            "Connected",
            device.Lanes.Count);
    }

    public async Task DisconnectAsync(Guid deviceId, CancellationToken ct = default)
    {
        var device = await _deviceRepository.GetByIdAsync(new DeviceId(deviceId), ct)
            ?? throw new InvalidOperationException("Device does not exist.");

        await _adapterFactory.UnregisterAdapterAsync(deviceId);

        device.Disconnect();
        await _deviceRepository.UpdateAsync(device, ct);
    }

    public async Task<IReadOnlyList<LaneDto>> GetLanesAsync(Guid deviceId, CancellationToken ct = default)
    {
        var device = await _deviceRepository.GetByIdAsync(new DeviceId(deviceId), ct)
            ?? throw new InvalidOperationException("Device does not exist.");

        return device.Lanes.Select(l => new LaneDto(
            l.Id,
            l.LaneIndex,
            l.LaneName,
            l.PgEnabled,
            l.EdEnabled,
            l.CurrentPattern
        )).ToList();
    }
}
