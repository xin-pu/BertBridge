using BertBridge.Application.Contracts;
using BertBridge.PluginSDK;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BertBridge.Infrastructure.PluginSystem;

public class DeviceAdapterFactory : IDeviceAdapterFactory
{
    private readonly Dictionary<Guid, IDeviceAdapter> _onlineAdapters = [];
    private readonly IReadOnlyList<IDeviceAdapter> _availableAdapters;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DeviceAdapterFactory> _logger;

    public DeviceAdapterFactory(
        IEnumerable<IDeviceAdapter> availableAdapters,
        IServiceProvider serviceProvider,
        ILogger<DeviceAdapterFactory> logger)
    {
        _availableAdapters = availableAdapters.ToList();
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public IDeviceAdapter CreateAdapter(ConnectionString connectionString)
    {
        var prototype = _availableAdapters.FirstOrDefault(a => a.CanHandle(connectionString));
        if (prototype == null)
            throw new InvalidOperationException($"No adapter can handle connection string: {connectionString}");

        return (IDeviceAdapter)ActivatorUtilities.CreateInstance(_serviceProvider, prototype.GetType());
    }

    public IDeviceAdapter? GetAdapter(Guid deviceId)
    {
        _onlineAdapters.TryGetValue(deviceId, out var adapter);
        return adapter;
    }

    public void RegisterAdapter(Guid deviceId, IDeviceAdapter adapter)
    {
        _onlineAdapters[deviceId] = adapter;
        _logger.LogInformation(
            "Adapter registered: DeviceId={DeviceId}, Adapter={AdapterType}",
            deviceId,
            adapter.GetType().Name);
    }

    public async ValueTask UnregisterAdapterAsync(Guid deviceId)
    {
        if (_onlineAdapters.Remove(deviceId, out var adapter))
        {
            await adapter.DisconnectAsync();
            await adapter.DisposeAsync();
            _logger.LogInformation("Adapter unregistered: DeviceId={DeviceId}", deviceId);
        }
    }

    public bool CanHandle(ConnectionString connectionString)
        => _availableAdapters.Any(a => a.CanHandle(connectionString));
}
