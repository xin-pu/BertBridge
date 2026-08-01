namespace BertBridge.PluginSDK;

/// <summary>
/// 连接协议枚举。
/// </summary>
public enum ConnectionProtocol
{
    Serial,
    Tcp
}

/// <summary>
/// 连接状态枚举。
/// </summary>
public enum ConnectionState
{
    Disconnected,
    Connecting,
    Connected,
    Error
}

/// <summary>
/// 连接字符串记录。支持串口 (COM3:115200) 和 TCP (192.168.1.1:5025)。
/// </summary>
public sealed record ConnectionString(
    ConnectionProtocol Protocol,
    string Value
)
{
    public static ConnectionString FromSerial(string portName, int baudRate)
        => new(ConnectionProtocol.Serial, $"{portName}:{baudRate}");

    public static ConnectionString FromTcp(string host, int port)
        => new(ConnectionProtocol.Tcp, $"{host}:{port}");

    /// <summary>
    /// 从原始字符串解析连接信息。
    /// </summary>
    public static ConnectionString Parse(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            throw new ArgumentException("连接字符串不能为空。", nameof(raw));

        if (!raw.StartsWith("COM", StringComparison.OrdinalIgnoreCase))
        {
            var parts = raw.Split(':');
            if (parts.Length == 2 && int.TryParse(parts[1], out var port))
                return FromTcp(parts[0], port);
        }

        var comParts = raw.Split(':');
        if (comParts.Length == 2 && comParts[0].StartsWith("COM", StringComparison.OrdinalIgnoreCase))
        {
            if (int.TryParse(comParts[1], out var baudRate))
                return FromSerial(comParts[0], baudRate);
        }

        throw new ArgumentException($"无法解析连接字符串: {raw}", nameof(raw));
    }

    public override string ToString() => Value;
}
