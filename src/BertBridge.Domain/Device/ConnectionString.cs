namespace BertBridge.Domain.Device;

/// <summary>
/// 连接字符串值对象。支持串口 (COM3:115200) 和 TCP (192.168.1.1:5025) 两种格式。
/// </summary>
public sealed class ConnectionString : Shared.ValueObject
{
    /// <summary>
    /// 连接协议类型。
    /// </summary>
    public ConnectionProtocol Protocol { get; }

    /// <summary>
    /// 原始连接字符串。
    /// </summary>
    public string Value { get; }

    private ConnectionString(ConnectionProtocol protocol, string value)
    {
        Protocol = protocol;
        Value = value;
    }

    /// <summary>
    /// 解析串口连接字符串，格式: COM3:115200
    /// </summary>
    public static ConnectionString FromSerial(string portName, int baudRate)
    {
        if (string.IsNullOrWhiteSpace(portName))
            throw new ArgumentException("端口名不能为空。", nameof(portName));
        if (baudRate <= 0)
            throw new ArgumentException("波特率必须大于零。", nameof(baudRate));

        return new ConnectionString(ConnectionProtocol.Serial, $"{portName}:{baudRate}");
    }

    /// <summary>
    /// 解析 TCP 连接字符串，格式: 192.168.1.1:5025
    /// </summary>
    public static ConnectionString FromTcp(string host, int port)
    {
        if (string.IsNullOrWhiteSpace(host))
            throw new ArgumentException("主机地址不能为空。", nameof(host));
        if (port <= 0 || port > 65535)
            throw new ArgumentException("端口号无效。", nameof(port));

        return new ConnectionString(ConnectionProtocol.Tcp, $"{host}:{port}");
    }

    /// <summary>
    /// 从原始连接字符串解析。
    /// </summary>
    public static ConnectionString Parse(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            throw new ArgumentException("连接字符串不能为空。", nameof(raw));

        // 尝试 TCP 格式: host:port (不含 COM 前缀)
        if (!raw.StartsWith("COM", StringComparison.OrdinalIgnoreCase))
        {
            var parts = raw.Split(':');
            if (parts.Length == 2 && int.TryParse(parts[1], out var port))
                return FromTcp(parts[0], port);
        }

        // 尝试串口格式: COM3:115200
        var comParts = raw.Split(':');
        if (comParts.Length == 2 && comParts[0].StartsWith("COM", StringComparison.OrdinalIgnoreCase))
        {
            if (int.TryParse(comParts[1], out var baudRate))
                return FromSerial(comParts[0], baudRate);
        }

        throw new ArgumentException($"无法解析连接字符串: {raw}", nameof(raw));
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Protocol;
        yield return Value;
    }

    public override string ToString() => Value;
}

/// <summary>
/// 连接协议枚举。
/// </summary>
public enum ConnectionProtocol
{
    Serial,
    Tcp
}
