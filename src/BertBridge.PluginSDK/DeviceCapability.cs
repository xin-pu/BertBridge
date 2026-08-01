namespace BertBridge.PluginSDK;

/// <summary>
/// 设备能力声明。描述设备支持的硬件功能集。
/// </summary>
public sealed record DeviceCapability(
    /// <summary>最大通道数</summary>
    int MaxLanes,

    /// <summary>是否支持 PAM4</summary>
    bool SupportsPAM4,

    /// <summary>是否支持高级调制 (PAM6/PAM8)</summary>
    bool SupportsAdvancedModulation,

    /// <summary>支持的 PRBS 码型列表</summary>
    IReadOnlyList<string> SupportedPatterns,

    /// <summary>最大波特率 (GBd)</summary>
    decimal MaxBaudRateGBd,

    /// <summary>是否支持 FEC</summary>
    bool SupportsFec,

    /// <summary>是否支持 GPIO</summary>
    bool SupportsGpio,

    /// <summary>FIR 抽头数</summary>
    int FirTapCount,

    /// <summary>是否支持抖动注入</summary>
    bool SupportsJitterInjection
);
