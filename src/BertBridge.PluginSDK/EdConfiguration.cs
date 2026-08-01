namespace BertBridge.PluginSDK;

/// <summary>
/// ED 配置记录。
/// </summary>
public sealed record EdConfiguration(
    string ExpectedPattern,
    bool AutoLock = true,
    double? BerThreshold = null,
    double? SnrThresholdDb = null
);
