namespace BertBridge.Domain.PatternGeneration;

/// <summary>
/// 固定码型值对象。用于用户自定义的比特序列。
/// </summary>
public sealed class FixedPattern : Shared.ValueObject
{
    /// <summary>比特序列（uint64 表示）</summary>
    public ulong BitSequence { get; }

    /// <summary>有效比特数（1-64）</summary>
    public int BitLength { get; }

    public FixedPattern(ulong bitSequence, int bitLength)
    {
        if (bitLength < 1 || bitLength > 64)
            throw new ArgumentException("比特长度必须在 1-64 之间。", nameof(bitLength));

        BitSequence = bitSequence;
        BitLength = bitLength;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return BitSequence;
        yield return BitLength;
    }

    public override string ToString() => $"0x{BitSequence:X} ({BitLength}-bit)";
}
