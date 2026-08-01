namespace BertBridge.Domain.Device;

/// <summary>
/// 设备连接状态。
/// </summary>
public enum ConnectionState
{
    /// <summary>未连接</summary>
    Disconnected,

    /// <summary>连接中</summary>
    Connecting,

    /// <summary>已连接</summary>
    Connected,

    /// <summary>连接错误</summary>
    Error
}
