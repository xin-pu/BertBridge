namespace BertBridge.Domain.Fec;

/// <summary>
/// FEC 配置值对象。
/// </summary>
public sealed class FecConfiguration : Shared.ValueObject
{
    /// <summary>FEC 类型</summary>
    public FecType Type { get; }

    /// <summary>FEC 模式：Bypass / Generate / DetectAndCorrect</summary>
    public FecMode Mode { get; }

    /// <summary>是否启用 FEC 感知统计</summary>
    public bool EnableFecAwareStats { get; }

    public FecConfiguration(FecType type, FecMode mode, bool enableFecAwareStats = false)
    {
        Type = type;
        Mode = mode;
        EnableFecAwareStats = enableFecAwareStats;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Type;
        yield return Mode;
        yield return EnableFecAwareStats;
    }
}

/// <summary>
/// FEC 工作模式。
/// </summary>
public enum FecMode
{
    /// <summary>直通模式（不处理 FEC）</summary>
    Bypass,

    /// <summary>生成 FEC 校验位</summary>
    Generate,

    /// <summary>检测并纠正错误</summary>
    DetectAndCorrect
}
