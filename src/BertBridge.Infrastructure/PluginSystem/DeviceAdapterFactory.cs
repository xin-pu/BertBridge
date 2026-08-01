using BertBridge.Application.Contracts;
using BertBridge.PluginSDK;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BertBridge.Infrastructure.PluginSystem;

public class DeviceAdapterFactory : IDeviceAdapterFactory
{
    private readonly Dictionary<Guid, IDeviceAdapter> _onlineAdapters = [];
    private readonly IReadOnlyList<DeviceAdapterDescriptor> _adapterDescriptors;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DeviceAdapterFactory> _logger;

    public DeviceAdapterFactory(
        IEnumerable<DeviceAdapterDescriptor> adapterDescriptors,
        IServiceProvider serviceProvider,
        ILogger<DeviceAdapterFactory> logger)
    {
        _adapterDescriptors = adapterDescriptors.ToList();
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public IDeviceAdapter CreateAdapter(ConnectionString connectionString)
    {
        var descriptor = _adapterDescriptors.FirstOrDefault(d => CanDescriptorHandle(d, connectionString));
        if (descriptor == null)
            throw new InvalidOperationException($"No adapter can handle connection string: {connectionString}");

        return (IDeviceAdapter)ActivatorUtilities.CreateInstance(_serviceProvider, descriptor.AdapterType);
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
        => _adapterDescriptors.Any(d => CanDescriptorHandle(d, connectionString));

    private bool CanDescriptorHandle(DeviceAdapterDescriptor descriptor, ConnectionString connectionString)
    {
        var adapter = (IDeviceAdapter)ActivatorUtilities.CreateInstance(_serviceProvider, descriptor.AdapterType);
        try
        {
            return adapter.CanHandle(connectionString);
        }
        finally
        {
            adapter.DisposeAsync().AsTask().GetAwaiter().GetResult();
        }
    }
}
