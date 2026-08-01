namespace BertBridge.PluginSDK;

/// <summary>
/// 标记插件入口点的特性。应用在实现 IDeviceAdapter 的类上。
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class AdapterRegistrationAttribute : Attribute
{
    /// <summary>插件显示名称</summary>
    public string Name { get; }

    /// <summary>插件版本</summary>
    public string Version { get; }

    /// <summary>厂商名称</summary>
    public string Vendor { get; }

    /// <summary>支持的设备型号（逗号分隔）</summary>
    public string SupportedModels { get; }

    public AdapterRegistrationAttribute(string name, string version, string vendor, string supportedModels)
    {
        Name = name;
        Version = version;
        Vendor = vendor;
        SupportedModels = supportedModels;
    }
}
