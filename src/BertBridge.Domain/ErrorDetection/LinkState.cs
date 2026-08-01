namespace BertBridge.Domain.ErrorDetection;

/// <summary>
/// 链路锁定状态。跟踪 ED 接收端的关键状态位。
/// </summary>
public sealed class LinkState : Shared.ValueObject
{
    /// <summary>信号检测 (Signal Detect)</summary>
    public bool SignalDetected { get; }

    /// <summary>CDR 时钟恢复锁定</summary>
    public bool CdrLocked { get; }

    /// <summary>PLL 锁相环锁定</summary>
    public bool PllLocked { get; }

    /// <summary>DSP 就绪</summary>
    public bool DspReady { get; }

    /// <summary>FEC 帧同步锁定</summary>
    public bool FecLocked { get; }

    /// <summary>Alignment Marker 锁定</summary>
    public bool AlignmentLocked { get; }

    /// <summary>所有状态位是否全部锁定</summary>
    public bool AllLocked => SignalDetected && CdrLocked && PllLocked && DspReady;

    public LinkState(
        bool signalDetected,
        bool cdrLocked,
        bool pllLocked,
        bool dspReady,
        bool fecLocked = false,
        bool alignmentLocked = false)
    {
        SignalDetected = signalDetected;
        CdrLocked = cdrLocked;
        PllLocked = pllLocked;
        DspReady = dspReady;
        FecLocked = fecLocked;
        AlignmentLocked = alignmentLocked;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return SignalDetected;
        yield return CdrLocked;
        yield return PllLocked;
        yield return DspReady;
        yield return FecLocked;
        yield return AlignmentLocked;
    }
}
