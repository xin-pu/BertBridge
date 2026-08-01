namespace BertBridge.Domain.PatternGeneration;

/// <summary>
/// 码型模式枚举。
/// </summary>
public enum PatternMode
{
    /// <summary>单码流输出（NRZ 标准模式）</summary>
    SingleStream,

    /// <summary>PAM4 MSB/LSB 独立码型</summary>
    MsbLsbIndependent,

    /// <summary>多段码型序列</summary>
    PatternSequencer,

    /// <summary>全 0 输出</summary>
    AllZero,

    /// <summary>全 1 输出</summary>
    AllOne
}
