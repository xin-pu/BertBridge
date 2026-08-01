using BertBridge.PluginSDK;
using Microsoft.Extensions.DependencyInjection;

namespace BertBridge.Plugins.Mock;

/// <summary>
/// Mock 插件 DI 注册模块。当 Mock 插件作为专项测试依赖时使用；
/// 生产环境下由 PluginDiscoveryService 从 Plugins/ 目录动态加载。
/// </summary>
public static class MockModule
{
    /// <summary>
    /// 注册 Mock 适配器到 DI 容器。
    /// </summary>
    public static IServiceCollection AddMockPlugin(this IServiceCollection services)
    {
        services.AddTransient<IDeviceAdapter, MockAdapter>();
        return services;
    }
}
