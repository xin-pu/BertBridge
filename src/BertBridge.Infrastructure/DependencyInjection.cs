using BertBridge.Application.Contracts;
using BertBridge.Domain.Device;
using BertBridge.Domain.TestSession;
using BertBridge.Infrastructure.Persistence;
using BertBridge.Infrastructure.Persistence.Repositories;
using BertBridge.Infrastructure.PluginSystem;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace BertBridge.Infrastructure;

/// <summary>
/// Infrastructure 层 DI 注册扩展。
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// 注册所有 Infrastructure 层服务。
    /// </summary>
    public static IServiceCollection AddBertBridgeInfrastructure(
        this IServiceCollection services, IConfiguration configuration)
    {
        // EF Core + SQLite
        var connectionString = configuration.GetConnectionString("Default")
            ?? "Data Source=bertbridge.db";
        services.AddDbContext<BertBridgeDbContext>(options =>
            options.UseSqlite(connectionString));

        // 仓储
        services.AddScoped<IDeviceRepository, DeviceRepository>();
        services.AddScoped<ITestSessionRepository, TestSessionRepository>();

        // 插件系统
        services.AddSingleton<PluginDiscoveryService>();
        services.AddSingleton<IDeviceAdapterFactory>(sp =>
        {
            var logger = sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<DeviceAdapterFactory>>();

            // 扫描并加载插件
            var discovery = sp.GetRequiredService<PluginDiscoveryService>();
            var pluginsPath = configuration["Plugins:Path"]
                ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Plugins");
            var adapters = discovery.DiscoverAdapters(pluginsPath);

            return new DeviceAdapterFactory(adapters, logger);
        });

        // 设备通信
        services.AddTransient<DeviceCommunication.IDeviceProtocol,
            DeviceCommunication.SerialDeviceProtocol>();

        // 设备发现
        services.AddTransient<DeviceCommunication.DeviceDiscoveryService>();

        // Serilog
        services.AddLogging(builder =>
        {
            Logging.SerilogConfiguration.Configure();
            builder.AddSerilog();
        });

        return services;
    }
}
