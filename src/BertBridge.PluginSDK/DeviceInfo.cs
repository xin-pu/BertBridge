namespace BertBridge.PluginSDK;

/// <summary>
/// 设备基本信息。
/// </summary>
public sealed record DeviceInfo(
    string Model,
    string SerialNumber,
    string FirmwareVersion,
    string BoardType
);
