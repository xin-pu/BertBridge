namespace BertBridge.Infrastructure.DeviceCommunication;

/// <summary>
/// 设备通信协议抽象接口。
/// </summary>
public interface IDeviceProtocol : IDisposable
{
    /// <summary>是否已连接</summary>
    bool IsConnected { get; }

    /// <summary>连接设备</summary>
    Task ConnectAsync(string connectionString, CancellationToken ct = default);

    /// <summary>断开连接</summary>
    Task DisconnectAsync();

    /// <summary>发送命令并接收响应</summary>
    Task<string> SendCommandAsync(string command, CancellationToken ct = default);

    /// <summary>发送原始数据</summary>
    Task SendRawAsync(byte[] data, CancellationToken ct = default);

    /// <summary>接收原始数据</summary>
    Task<byte[]> ReceiveRawAsync(int expectedLength, CancellationToken ct = default);
}
