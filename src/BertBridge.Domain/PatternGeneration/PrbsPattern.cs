namespace BertBridge.Domain.PatternGeneration;

/// <summary>
/// PRBS 码型枚举。涵盖 ITU-T O.150/O.151/O.153 标准定义的所有常用码型，
/// 以及 Intel FPGA 专用 PRBS58 和 QPRBS 变体。
/// </summary>
public enum PrbsPattern
{
    PRBS7,
    PRBS9,
    PRBS11,
    PRBS13,
    PRBS15,
    PRBS16,
    PRBS20,
    PRBS23,
    PRBS31,
    PRBS58,
    PRBS7Q,
    PRBS9Q,
    PRBS11Q,
    PRBS13Q,
    PRBS15Q,
    PRBS23Q,
    PRBS31Q,
    SSPRQ,
    SSPR,
    SquareWave,
    ClockPattern,
    Custom
}
