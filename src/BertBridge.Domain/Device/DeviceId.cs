namespace BertBridge.Domain.Device;

/// <summary>
/// 设备聚合的唯一标识值对象。
/// </summary>
public sealed class DeviceId : Shared.ValueObject
{
    public Guid Value { get; }

    public DeviceId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("DeviceId 不能为空 GUID。", nameof(value));
        Value = value;
    }

    public static DeviceId New() => new(Guid.NewGuid());

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value.ToString();
}
