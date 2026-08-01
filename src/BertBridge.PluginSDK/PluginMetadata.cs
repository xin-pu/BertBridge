namespace BertBridge.PluginSDK;

/// <summary>
/// 插件元数据。描述插件的基本信息和能力范围。
/// </summary>
public sealed record PluginMetadata(
    /// <summary>插件显示名称</summary>
    string Name,

    /// <summary>插件版本</summary>
    string Version,

    /// <summary>厂商名称</summary>
    string Vendor,

    /// <summary>支持的设备型号列表</summary>
    IReadOnlyList<string> SupportedModels,

    /// <summary>支持的通信协议 (Serial/Tcp)</summary>
    IReadOnlyList<string> SupportedProtocols,

    /// <summary>插件描述</summary>
    string? Description = null
);
