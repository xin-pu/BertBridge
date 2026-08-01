using BertBridge.PluginSDK;
using McMaster.NETCore.Plugins;
using Microsoft.Extensions.Logging;

namespace BertBridge.Infrastructure.PluginSystem;

/// <summary>
/// Discovers external adapter plugins from the configured Plugins directory.
/// </summary>
public class PluginDiscoveryService
{
    private readonly List<PluginLoader> _loaders = [];
    private readonly List<DeviceAdapterDescriptor> _descriptors = [];
    private readonly ILogger<PluginDiscoveryService> _logger;

    public PluginDiscoveryService(ILogger<PluginDiscoveryService> logger)
    {
        _logger = logger;
    }

    public IReadOnlyList<DeviceAdapterDescriptor> DiscoverAdapterDescriptors(string pluginsPath)
    {
        if (!Directory.Exists(pluginsPath))
        {
            _logger.LogWarning("插件目录不存在: {Path}", pluginsPath);
            return _descriptors;
        }

        foreach (var pluginDir in Directory.GetDirectories(pluginsPath))
        {
            DiscoverFromDirectory(pluginDir);
        }

        return _descriptors;
    }

    public void UnloadAll()
    {
        foreach (var loader in _loaders)
        {
            loader.Dispose();
        }

        _loaders.Clear();
        _descriptors.Clear();
        _logger.LogInformation("所有插件已卸载");
    }

    private void DiscoverFromDirectory(string pluginDir)
    {
        try
        {
            var pluginDll = Directory.GetFiles(pluginDir, "*.dll")
                .FirstOrDefault(f => Path.GetFileName(f).StartsWith("BertBridge.Plugins."));

            if (pluginDll == null)
                return;

            _logger.LogInformation("发现插件 DLL: {Dll}", pluginDll);

            var loader = PluginLoader.CreateFromAssemblyFile(
                pluginDll,
                config => config.PreferSharedTypes = true);
            var assembly = loader.LoadDefaultAssembly();

            var descriptors = assembly.GetTypes()
                .Where(type => typeof(IDeviceAdapter).IsAssignableFrom(type) && !type.IsAbstract)
                .Select(type => DeviceAdapterDescriptor.FromAdapterType(type, pluginDll))
                .ToList();

            if (descriptors.Count == 0)
            {
                loader.Dispose();
                return;
            }

            _loaders.Add(loader);
            _descriptors.AddRange(descriptors);

            foreach (var descriptor in descriptors)
            {
                _logger.LogInformation(
                    "加载插件适配器: {Name} v{Version} by {Vendor} ({Type})",
                    descriptor.Name,
                    descriptor.Version,
                    descriptor.Vendor,
                    descriptor.AdapterType.Name);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "加载插件失败: {PluginDir}", pluginDir);
        }
    }
}
