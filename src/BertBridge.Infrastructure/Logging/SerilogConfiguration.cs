using Serilog;
using Serilog.Events;

namespace BertBridge.Infrastructure.Logging;

/// <summary>
/// Serilog 日志配置。
/// </summary>
public static class SerilogConfiguration
{
    /// <summary>
    /// 创建默认的 Serilog Logger 配置。
    /// </summary>
    public static void Configure()
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .WriteTo.Console(outputTemplate:
                "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
            .WriteTo.File("logs/bertbridge-.log",
                rollingInterval: RollingInterval.Day,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();
    }
}
