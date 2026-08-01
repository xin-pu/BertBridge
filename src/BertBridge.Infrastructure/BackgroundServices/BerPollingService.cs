using BertBridge.Application.Contracts;
using BertBridge.PluginSDK;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BertBridge.Infrastructure.BackgroundServices;

/// <summary>
/// BER 轮询后台服务。定期从已连接设备读取 ED 结果。
/// </summary>
public class BerPollingService : BackgroundService
{
    private readonly IDeviceAdapterFactory _adapterFactory;
    private readonly ILogger<BerPollingService> _logger;
    private readonly TimeSpan _pollingInterval = TimeSpan.FromSeconds(2);

    public BerPollingService(IDeviceAdapterFactory adapterFactory, ILogger<BerPollingService> logger)
    {
        _adapterFactory = adapterFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("BER 轮询服务已启动，间隔: {Interval}", _pollingInterval);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // TODO: 从已注册的设备池获取活跃设备列表
                await Task.Delay(_pollingInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "BER 轮询异常");
            }
        }

        _logger.LogInformation("BER 轮询服务已停止");
    }
}
