namespace BertBridge.Domain.Gpio;

/// <summary>
/// GPIO 控制器实体。管理设备 GPIO 端口的读写。
/// </summary>
public class GpioController : Shared.BaseEntity
{
    /// <summary>GPIO 端口宽度（典型值 24）</summary>
    public int PortWidth { get; }

    /// <summary>当前引脚映射（按 PinNumber 索引）</summary>
    public IReadOnlyList<GpioPin> Pins { get; private set; }

    /// <summary>当前端口原始值</summary>
    public uint RawValue { get; private set; }

    internal GpioController(Guid id, int portWidth, IEnumerable<GpioPin> pins) : base(id)
    {
        if (portWidth <= 0)
            throw new ArgumentException("端口宽度必须大于零。", nameof(portWidth));

        PortWidth = portWidth;
        Pins = pins?.ToList().AsReadOnly()
            ?? throw new ArgumentNullException(nameof(pins));
        RawValue = 0;
    }

    /// <summary>
    /// 更新单个引脚状态。
    /// </summary>
    internal void SetPin(int pinNumber, GpioPinState state)
    {
        if (pinNumber >= PortWidth)
            throw new ArgumentOutOfRangeException(nameof(pinNumber));

        // 更新原始值中的对应位
        if (state == GpioPinState.High)
            RawValue |= (uint)(1 << pinNumber);
        else
            RawValue &= ~(uint)(1 << pinNumber);
    }

    /// <summary>
    /// 批量更新端口值（通过 mask 和 values）。
    /// </summary>
    internal void WritePort(uint mask, uint values)
    {
        RawValue = (RawValue & ~mask) | (values & mask);
    }

    /// <summary>
    /// 读取端口原始值。
    /// </summary>
    internal uint ReadPort()
    {
        return RawValue;
    }
}
