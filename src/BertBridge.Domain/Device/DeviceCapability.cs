namespace BertBridge.Domain.Device;

/// <summary>
/// 设备能力声明值对象。描述设备支持的硬件能力集。
/// </summary>
public sealed class DeviceCapability : Shared.ValueObject
{
    /// <summary>最大通道数</summary>
    public int MaxLanes { get; }

    /// <summary>是否支持 PAM4 信号制式</summary>
    public bool SupportsPAM4 { get; }

    /// <summary>是否支持 PAM6/PAM8 信号制式</summary>
    public bool SupportsAdvancedModulation { get; }

    /// <summary>支持的所有 PRBS 码型</summary>
    public IReadOnlyList<string> SupportedPatterns { get; }

    /// <summary>最大波特率 (GBd)</summary>
    public decimal MaxBaudRateGBd { get; }

    /// <summary>是否支持 FEC 分析</summary>
    public bool SupportsFec { get; }

    /// <summary>是否支持 GPIO</summary>
    public bool SupportsGpio { get; }

    /// <summary>FIR 抽头数（0 表示不支持）</summary>
    public int FirTapCount { get; }

    /// <summary>是否支持抖动注入</summary>
    public bool SupportsJitterInjection { get; }

    public DeviceCapability(
        int maxLanes,
        bool supportsPAM4,
        bool supportsAdvancedModulation,
        IReadOnlyList<string> supportedPatterns,
        decimal maxBaudRateGBd,
        bool supportsFec,
        bool supportsGpio,
        int firTapCount,
        bool supportsJitterInjection)
    {
        if (maxLanes <= 0)
            throw new ArgumentException("最大通道数必须大于零。", nameof(maxLanes));
        if (maxBaudRateGBd <= 0)
            throw new ArgumentException("最大波特率必须大于零。", nameof(maxBaudRateGBd));

        MaxLanes = maxLanes;
        SupportsPAM4 = supportsPAM4;
        SupportsAdvancedModulation = supportsAdvancedModulation;
        SupportedPatterns = supportedPatterns?.ToList().AsReadOnly()
            ?? throw new ArgumentNullException(nameof(supportedPatterns));
        MaxBaudRateGBd = maxBaudRateGBd;
        SupportsFec = supportsFec;
        SupportsGpio = supportsGpio;
        FirTapCount = firTapCount;
        SupportsJitterInjection = supportsJitterInjection;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return MaxLanes;
        yield return SupportsPAM4;
        yield return MaxBaudRateGBd;
        yield return SupportsFec;
        yield return SupportsGpio;
    }
}
