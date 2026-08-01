namespace BertBridge.Domain.Device;

/// <summary>
/// 设备基本信息值对象（型号、序列号、固件版本、板卡类型）。
/// </summary>
public sealed class DeviceInfo : Shared.ValueObject
{
    public string Model { get; }
    public string SerialNumber { get; }
    public string FirmwareVersion { get; }
    public string BoardType { get; }

    public DeviceInfo(string model, string serialNumber, string firmwareVersion, string boardType)
    {
        Model = model ?? throw new ArgumentNullException(nameof(model));
        SerialNumber = serialNumber ?? throw new ArgumentNullException(nameof(serialNumber));
        FirmwareVersion = firmwareVersion ?? throw new ArgumentNullException(nameof(firmwareVersion));
        BoardType = boardType ?? throw new ArgumentNullException(nameof(boardType));
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Model;
        yield return SerialNumber;
        yield return FirmwareVersion;
        yield return BoardType;
    }

    public override string ToString() => $"{Model} (SN:{SerialNumber}, FW:{FirmwareVersion})";
}
