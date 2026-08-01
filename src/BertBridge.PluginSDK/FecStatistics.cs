namespace BertBridge.PluginSDK;

/// <summary>
/// FEC 统计记录。
/// </summary>
public sealed record FecStatistics(
    double? PreFecBer,
    double? PostFecBer,
    ulong CorrectableCodewords,
    ulong UncorrectableCodewords,
    ulong SymbolErrors,
    bool IsLocked,
    DateTime Timestamp
);
