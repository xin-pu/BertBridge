namespace BertBridge.Domain.ErrorDetection;

/// <summary>
/// SNR 值对象。
/// </summary>
public sealed class SnrValue : Shared.ValueObject
{
    /// <summary>原始 SNR 值（DSP 读取的无符号整数）</summary>
    public ushort RawValue { get; }

    /// <summary>SNR 转换值 (dB)</summary>
    public double Decibels { get; }

    public SnrValue(ushort rawValue, double decibels)
    {
        RawValue = rawValue;
        Decibels = decibels;
    }

    /// <summary>
    /// 从原始 SNR 值转换为 dB。
    /// SNR_dB = 37.1 - 10 × log₁₀(SNR_ushort)
    /// </summary>
    public static SnrValue FromRaw(ushort rawValue)
    {
        double db = rawValue > 0
            ? 37.1 - 10.0 * Math.Log10(rawValue)
            : 0;
        return new SnrValue(rawValue, db);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return RawValue;
        yield return Decibels;
    }

    public override string ToString() => $"{Decibels:F2} dB";
}
