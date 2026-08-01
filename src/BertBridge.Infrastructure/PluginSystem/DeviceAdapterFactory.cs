using BertBridge.Application.Contracts;
using BertBridge.PluginSDK;
using Microsoft.Extensions.Logging;

namespace BertBridge.Infrastructure.PluginSystem;

/// <summary>
/// 设备适配器工厂实现。管理 DeviceId → IDeviceAdapter 的映射。
/// </summary>
public class DeviceAdapterFactory : IDeviceAdapterFactory
{
    private readonly Dictionary<Guid, IDeviceAdapter> _adapters = [];
    private readonly IReadOnlyList<IDeviceAdapter> _availableAdapters;
    private readonly ILogger<DeviceAdapterFactory> _logger;

    public DeviceAdapterFactory(IEnumerable<IDeviceAdapter> availableAdapters, ILogger<DeviceAdapterFactory> logger)
    {
        _availableAdapters = availableAdapters.ToList();
        _logger = logger;
    }

    public IDeviceAdapter? GetAdapter(Guid deviceId)
    {
        _adapters.TryGetValue(deviceId, out var adapter);
        return adapter;
    }

    public void RegisterAdapter(Guid deviceId, IDeviceAdapter adapter)
    {
        _adapters[deviceId] = adapter;
        _logger.LogInformation("适配器已注册: DeviceId={DeviceId}, Adapter={AdapterType}",
            deviceId, adapter.GetType().Name);
    }

    public void UnregisterAdapter(Guid deviceId)
    {
        if (_adapters.Remove(deviceId, out var adapter))
        {
            _logger.LogInformation("适配器已注销: DeviceId={DeviceId}", deviceId);
        }
    }

    public bool CanHandle(PluginSDK.ConnectionString connectionString)
    {
        return _availableAdapters.Any();
    }
}
