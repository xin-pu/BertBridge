using BertBridge.Infrastructure.DeviceCommunication;
using BertBridge.PluginSDK;
using Microsoft.Extensions.Logging;

namespace BertBridge.Infrastructure.DeviceCommunication;

/// <summary>
/// 设备发现服务。扫描可用端口并识别 BERT 设备。
/// </summary>
public class DeviceDiscoveryService
{
    private readonly ILogger<DeviceDiscoveryService> _logger;

    public DeviceDiscoveryService(ILogger<DeviceDiscoveryService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// 扫描可用串口设备。
    /// </summary>
    public Task<IReadOnlyList<string>> DiscoverSerialPortsAsync(CancellationToken ct = default)
    {
        var ports = System.IO.Ports.SerialPort.GetPortNames();
        _logger.LogInformation("发现 {Count} 个串口: {Ports}", ports.Length, string.Join(", ", ports));
        return Task.FromResult<IReadOnlyList<string>>(ports);
    }

    /// <summary>
    /// 尝试识别指定端口的设备类型。
    /// </summary>
    public async Task<string?> IdentifyDeviceAsync(string portName, CancellationToken ct = default)
    {
        try
        {
            using var protocol = new SerialDeviceProtocol(
                new LoggerFactory().CreateLogger<SerialDeviceProtocol>());

            await protocol.ConnectAsync($"{portName}:115200", ct);
            var response = await protocol.SendCommandAsync("*IDN?", ct);
            await protocol.DisconnectAsync();

            _logger.LogInformation("设备识别: {Port} → {Response}", portName, response);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "设备识别失败: {Port}", portName);
            return null;
        }
    }
}
