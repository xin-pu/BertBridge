using System.IO.Ports;
using Microsoft.Extensions.Logging;

namespace BertBridge.Infrastructure.DeviceCommunication;

/// <summary>
/// 串口通信协议实现。
/// </summary>
public class SerialDeviceProtocol : IDeviceProtocol
{
    private SerialPort? _serialPort;
    private readonly ILogger<SerialDeviceProtocol> _logger;

    public bool IsConnected => _serialPort?.IsOpen ?? false;

    public SerialDeviceProtocol(ILogger<SerialDeviceProtocol> logger)
    {
        _logger = logger;
    }

    public Task ConnectAsync(string connectionString, CancellationToken ct = default)
    {
        // 解析 COM3:115200 格式
        var parts = connectionString.Split(':');
        if (parts.Length != 2)
            throw new ArgumentException($"无效的串口连接字符串: {connectionString}");

        var portName = parts[0];
        var baudRate = int.Parse(parts[1]);

        _serialPort = new SerialPort(portName, baudRate)
        {
            ReadTimeout = 5000,
            WriteTimeout = 5000,
            DataBits = 8,
            Parity = Parity.None,
            StopBits = StopBits.One
        };

        _serialPort.Open();
        _logger.LogInformation("串口已连接: {Port}@{BaudRate}", portName, baudRate);
        return Task.CompletedTask;
    }

    public Task DisconnectAsync()
    {
        _serialPort?.Close();
        _serialPort?.Dispose();
        _serialPort = null;
        _logger.LogInformation("串口已断开");
        return Task.CompletedTask;
    }

    public async Task<string> SendCommandAsync(string command, CancellationToken ct = default)
    {
        if (_serialPort == null || !_serialPort.IsOpen)
            throw new InvalidOperationException("串口未连接。");

        _serialPort.WriteLine(command);
        return await Task.Run(() => _serialPort.ReadLine(), ct);
    }

    public async Task SendRawAsync(byte[] data, CancellationToken ct = default)
    {
        if (_serialPort == null || !_serialPort.IsOpen)
            throw new InvalidOperationException("串口未连接。");

        await _serialPort.BaseStream.WriteAsync(data, ct);
        await _serialPort.BaseStream.FlushAsync(ct);
    }

    public async Task<byte[]> ReceiveRawAsync(int expectedLength, CancellationToken ct = default)
    {
        if (_serialPort == null || !_serialPort.IsOpen)
            throw new InvalidOperationException("串口未连接。");

        var buffer = new byte[expectedLength];
        var bytesRead = await _serialPort.BaseStream.ReadAsync(buffer, ct);
        return buffer[..bytesRead];
    }

    public void Dispose()
    {
        _serialPort?.Dispose();
    }
}
