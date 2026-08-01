namespace BertBridge.Domain.Gpio;

/// <summary>
/// GPIO 引脚值对象。
/// </summary>
public sealed class GpioPin : Shared.ValueObject
{
    /// <summary>引脚编号 (0-based)</summary>
    public int PinNumber { get; }

    /// <summary>引脚名称</summary>
    public string PinName { get; }

    /// <summary>当前状态</summary>
    public GpioPinState State { get; }

    /// <summary>是否为输出引脚</summary>
    public bool IsOutput { get; }

    public GpioPin(int pinNumber, string pinName, GpioPinState state, bool isOutput)
    {
        if (pinNumber < 0)
            throw new ArgumentException("引脚编号不能为负数。", nameof(pinNumber));
        if (string.IsNullOrWhiteSpace(pinName))
            throw new ArgumentException("引脚名称不能为空。", nameof(pinName));

        PinNumber = pinNumber;
        PinName = pinName;
        State = state;
        IsOutput = isOutput;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return PinNumber;
        yield return PinName;
        yield return State;
        yield return IsOutput;
    }

    public override string ToString() => $"Pin{PinNumber}({PinName}): {State}";
}
