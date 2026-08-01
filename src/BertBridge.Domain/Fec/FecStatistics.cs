namespace BertBridge.Domain.Fec;

/// <summary>
/// FEC 统计值对象。封装 FEC 解码后的各项统计指标。
/// </summary>
public sealed class FecStatistics : Shared.ValueObject
{
    /// <summary>Pre-FEC BER（FEC 纠正前的物理误码率）</summary>
    public ErrorDetection.BerValue? PreFecBer { get; }

    /// <summary>Post-FEC BER / FLR（FEC 纠正后的误码率）</summary>
    public ErrorDetection.BerValue? PostFecBer { get; }

    /// <summary>可纠正码字数</summary>
    public ulong CorrectableCodewords { get; }

    /// <summary>不可纠正码字数</summary>
    public ulong UncorrectableCodewords { get; }

    /// <summary>符号错误总数</summary>
    public ulong SymbolErrors { get; }

    /// <summary>FEC 锁定状态</summary>
    public bool IsLocked { get; }

    /// <summary>统计时间戳</summary>
    public DateTime Timestamp { get; }

    public FecStatistics(
        ErrorDetection.BerValue? preFecBer,
        ErrorDetection.BerValue? postFecBer,
        ulong correctableCodewords,
        ulong uncorrectableCodewords,
        ulong symbolErrors,
        bool isLocked,
        DateTime timestamp)
    {
        PreFecBer = preFecBer;
        PostFecBer = postFecBer;
        CorrectableCodewords = correctableCodewords;
        UncorrectableCodewords = uncorrectableCodewords;
        SymbolErrors = symbolErrors;
        IsLocked = isLocked;
        Timestamp = timestamp;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return CorrectableCodewords;
        yield return UncorrectableCodewords;
        yield return SymbolErrors;
        yield return IsLocked;
        yield return Timestamp;
    }
}
