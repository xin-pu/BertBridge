using BertBridge.PluginSDK;
using McMaster.NETCore.Plugins;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BertBridge.Infrastructure.PluginSystem;

/// <summary>
/// 插件发现服务。扫描 Plugins/ 目录，使用 McMaster.NETCore.Plugins 加载适配器。
/// 支持通过 IServiceProvider 创建带 DI 依赖的适配器实例。
/// </summary>
public class PluginDiscoveryService
{
    private readonly List<PluginLoader> _loaders = [];
    private readonly List<IDeviceAdapter> _adapters = [];
    private readonly ILogger<PluginDiscoveryService> _logger;
    private readonly IServiceProvider _serviceProvider;

    public PluginDiscoveryService(ILogger<PluginDiscoveryService> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// 扫描并加载所有插件。
    /// </summary>
    public IReadOnlyList<IDeviceAdapter> DiscoverAdapters(string pluginsPath)
    {
        if (!Directory.Exists(pluginsPath))
        {
            _logger.LogWarning("插件目录不存在: {Path}", pluginsPath);
            return _adapters;
        }

        foreach (var pluginDir in Directory.GetDirectories(pluginsPath))
        {
            try
            {
                var dllFiles = Directory.GetFiles(pluginDir, "*.dll");
                var pluginDll = dllFiles.FirstOrDefault(f =>
                    Path.GetFileName(f).StartsWith("BertBridge.Plugins."));

                if (pluginDll == null) continue;

                _logger.LogInformation("发现插件 DLL: {Dll}", pluginDll);

                var loader = PluginLoader.CreateFromAssemblyFile(pluginDll,
                    config => config.PreferSharedTypes = true);
                var assembly = loader.LoadDefaultAssembly();

                foreach (var type in assembly.GetTypes())
                {
                    if (typeof(IDeviceAdapter).IsAssignableFrom(type) && !type.IsAbstract)
                    {
                        var attr = type.GetCustomAttributes(typeof(AdapterRegistrationAttribute), false)
                            .FirstOrDefault() as AdapterRegistrationAttribute;

                        if (attr != null)
                        {
                            _logger.LogInformation("加载插件: {Name} v{Version} by {Vendor}",
                                attr.Name, attr.Version, attr.Vendor);
                        }

                        // 使用 ActivatorUtilities 支持带 DI 依赖的适配器
                        if (ActivatorUtilities.CreateInstance(_serviceProvider, type) is IDeviceAdapter adapter)
                        {
                            _loaders.Add(loader);
                            _adapters.Add(adapter);
                            _logger.LogInformation("适配器已加载: {Type}", type.Name);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "加载插件失败: {PluginDir}", pluginDir);
            }
        }

        return _adapters;
    }

    /// <summary>
    /// 卸载所有已加载的插件。
    /// </summary>
    public void UnloadAll()
    {
        foreach (var loader in _loaders)
        {
            loader.Dispose();
        }
        _loaders.Clear();
        _adapters.Clear();
        _logger.LogInformation("所有插件已卸载");
    }
}
