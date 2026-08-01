namespace BertBridge.Domain.PatternGeneration;

/// <summary>
/// 发射摆幅值对象。单位为 mV (差分)。
/// </summary>
public sealed class TxSwing : Shared.ValueObject
{
    /// <summary>摆幅值 (mV, 差分峰峰值)</summary>
    public int Millivolts { get; }

    public TxSwing(int millivolts)
    {
        if (millivolts <= 0 || millivolts > 3000)
            throw new ArgumentException("摆幅必须在 1 ~ 3000 mV 之间。", nameof(millivolts));

        Millivolts = millivolts;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Millivolts;
    }

    public override string ToString() => $"{Millivolts} mV";
}
