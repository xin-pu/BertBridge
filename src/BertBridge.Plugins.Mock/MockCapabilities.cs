using BertBridge.PluginSDK;

namespace BertBridge.Plugins.Mock;

/// <summary>
/// Mock 设备能力声明。模拟一个 8 通道 PAM4 完整功能 BERT 设备。
/// </summary>
public static class MockCapabilities
{
    public static readonly DeviceCapability Capability = new(
        MaxLanes: 8,
        SupportsPAM4: true,
        SupportsAdvancedModulation: false,
        SupportedPatterns:
        [
            "PRBS7", "PRBS9", "PRBS11", "PRBS13", "PRBS15",
            "PRBS20", "PRBS23", "PRBS31",
            "PRBS7Q", "PRBS13Q", "PRBS31Q", "SSPRQ",
            "SquareWave", "ClockPattern",
            "Custom64", "Custom128"
        ],
        MaxBaudRateGBd: 56.0m,
        SupportsFec: true,
        SupportsGpio: true,
        FirTapCount: 5,
        SupportsJitterInjection: false
    );
}
