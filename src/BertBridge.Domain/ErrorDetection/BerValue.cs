namespace BertBridge.Domain.ErrorDetection;

/// <summary>
/// BER 值对象。使用科学计数法表示（mantissa × 10^exponent）。
/// 例如 1E-12 表示为 Mantissa=1, Exponent=-12。
/// </summary>
public sealed class BerValue : Shared.ValueObject
{
    /// <summary>尾数</summary>
    public double Mantissa { get; }

    /// <summary>指数</summary>
    public int Exponent { get; }

    /// <summary>BER 是否为零（无误码）</summary>
    public bool IsZero => ErrorCount == 0;

    /// <summary>错误比特数</summary>
    public ulong ErrorCount { get; }

    /// <summary>总比特数</summary>
    public ulong TotalCount { get; }

    public BerValue(double mantissa, int exponent, ulong errorCount, ulong totalCount)
    {
        if (mantissa <= 0 && errorCount > 0)
            throw new ArgumentException("当存在误码时，尾数必须大于零。", nameof(mantissa));

        Mantissa = mantissa;
        Exponent = exponent;
        ErrorCount = errorCount;
        TotalCount = totalCount;
    }

    /// <summary>
    /// 从错误计数和总计数计算 BER。
    /// </summary>
    public static BerValue Calculate(ulong errorCount, ulong totalCount)
    {
        if (totalCount == 0)
            return new BerValue(0, 0, 0, 0);

        if (errorCount == 0)
            return new BerValue(0, 0, 0, totalCount);

        double ber = (double)errorCount / totalCount;
        int exponent = (int)Math.Floor(Math.Log10(ber));
        double mantissa = ber / Math.Pow(10, exponent);

        return new BerValue(mantissa, exponent, errorCount, totalCount);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Mantissa;
        yield return Exponent;
        yield return ErrorCount;
        yield return TotalCount;
    }

    public override string ToString() => $"{Mantissa:F2}E{Exponent}";
}
