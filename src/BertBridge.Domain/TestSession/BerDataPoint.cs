namespace BertBridge.Domain.TestSession;

/// <summary>
/// BER 采样数据点实体。属于 TestSession 聚合。
/// </summary>
public class BerDataPoint : Shared.BaseEntity
{
    /// <summary>所属测试会话 ID</summary>
    public Guid TestSessionId { get; }

    /// <summary>通道索引</summary>
    public int LaneIndex { get; }

    /// <summary>采样时间戳 (Unix ms)</summary>
    public long TimestampMs { get; }

    /// <summary>错误比特数</summary>
    public ulong ErrorCount { get; }

    /// <summary>总比特数</summary>
    public ulong TotalCount { get; }

    /// <summary>BER 值（科学计数法 mantissa × 10^exponent）</summary>
    public double Ber { get; }

    /// <summary>SNR 值 (dB)</summary>
    public double? Snr { get; }

    internal BerDataPoint(
        Guid id,
        Guid testSessionId,
        int laneIndex,
        long timestampMs,
        ulong errorCount,
        ulong totalCount,
        double ber,
        double? snr) : base(id)
    {
        TestSessionId = testSessionId;
        LaneIndex = laneIndex;
        TimestampMs = timestampMs;
        ErrorCount = errorCount;
        TotalCount = totalCount;
        Ber = ber;
        Snr = snr;
    }
}
