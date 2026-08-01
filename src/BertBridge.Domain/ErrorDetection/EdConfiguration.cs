namespace BertBridge.Domain.ErrorDetection;

/// <summary>
/// ED 配置值对象。
/// </summary>
public sealed class EdConfiguration : Shared.ValueObject
{
    /// <summary>锁定期望的 PRBS 码型</summary>
    public PatternGeneration.PrbsPattern ExpectedPattern { get; }

    /// <summary>是否自动锁定（自动识别码型）</summary>
    public bool AutoLock { get; }

    /// <summary>BER 告警阈值（超过此值触发 BerThresholdExceededEvent）</summary>
    public BerValue? BerThreshold { get; }

    /// <summary>SNR 告警阈值 (dB)</summary>
    public double? SnrThresholdDb { get; }

    public EdConfiguration(
        PatternGeneration.PrbsPattern expectedPattern,
        bool autoLock = true,
        BerValue? berThreshold = null,
        double? snrThresholdDb = null)
    {
        ExpectedPattern = expectedPattern;
        AutoLock = autoLock;
        BerThreshold = berThreshold;
        SnrThresholdDb = snrThresholdDb;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return ExpectedPattern;
        yield return AutoLock;
    }
}
