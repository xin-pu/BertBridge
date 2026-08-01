namespace BertBridge.Domain.ErrorDetection;

/// <summary>
/// ED 结果值对象。封装单次误码检测的完整统计信息。
/// </summary>
public sealed class EdResult : Shared.ValueObject
{
    /// <summary>BER 值</summary>
    public BerValue Ber { get; }

    /// <summary>SNR 值</summary>
    public SnrValue? Snr { get; }

    /// <summary>链路状态</summary>
    public LinkState LinkState { get; }

    /// <summary>采样时间戳 (UTC)</summary>
    public DateTime Timestamp { get; }

    public EdResult(BerValue ber, SnrValue? snr, LinkState linkState, DateTime timestamp)
    {
        Ber = ber ?? throw new ArgumentNullException(nameof(ber));
        Snr = snr;
        LinkState = linkState ?? throw new ArgumentNullException(nameof(linkState));
        Timestamp = timestamp;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Ber;
        yield return Timestamp;
    }
}
