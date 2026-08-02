namespace BertBridge.PluginSDK;

/// <summary>
/// ED 结果记录。
/// </summary>
public sealed record EdResult(
    ulong ErrorCount,
    ulong TotalCount,
    double Ber,
    double? SnrDb,
    bool SignalDetected,
    bool CdrLocked,
    bool PllLocked,
    bool DspReady,
    bool FecLocked,
    bool AlignmentLocked,
    DateTime Timestamp
);
