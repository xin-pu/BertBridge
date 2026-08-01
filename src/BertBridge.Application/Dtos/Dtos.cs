namespace BertBridge.Application.Dtos;

/// <summary>
/// 设备 DTO。
/// </summary>
public record DeviceDto(
    Guid Id,
    string DeviceName,
    string? Model,
    string? SerialNumber,
    string? FirmwareVersion,
    string? ConnectionString,
    string ConnectionState,
    int LaneCount
);

/// <summary>
/// 设备列表项 DTO（摘要信息）。
/// </summary>
public record DeviceListItemDto(
    Guid Id,
    string DeviceName,
    string? Model,
    string ConnectionState
);

/// <summary>
/// 通道 DTO。
/// </summary>
public record LaneDto(
    Guid Id,
    int LaneIndex,
    string LaneName,
    bool PgEnabled,
    bool EdEnabled,
    string? CurrentPattern
);

/// <summary>
/// PG 配置 DTO。
/// </summary>
public record PgConfigurationDto(
    string Pattern,
    string Mode,
    string? CustomPattern,
    string? MsbPattern,
    string? LsbPattern,
    decimal[]? FirTaps,
    int? SwingMillivolts,
    bool GrayEncoding,
    bool PolarityInvert,
    bool PreCoding
);

/// <summary>
/// ED 结果 DTO。
/// </summary>
public record EdResultDto(
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

/// <summary>
/// FEC 统计 DTO。
/// </summary>
public record FecStatisticsDto(
    double? PreFecBer,
    double? PostFecBer,
    ulong CorrectableCodewords,
    ulong UncorrectableCodewords,
    ulong SymbolErrors,
    bool IsLocked,
    DateTime Timestamp
);

/// <summary>
/// 测试配置 DTO。
/// </summary>
public record TestConfigurationDto(
    Guid DeviceId,
    int LaneCount,
    string PatternsJson,
    TimeSpan? Duration
);

/// <summary>
/// 创建测试会话 DTO。
/// </summary>
public record CreateTestSessionDto(
    Guid DeviceId,
    int LaneCount,
    string PatternsJson,
    TimeSpan? Duration
);

/// <summary>
/// 测试会话 DTO。
/// </summary>
public record TestSessionDto(
    Guid Id,
    Guid DeviceId,
    string Status,
    DateTime? StartedAt,
    DateTime? CompletedAt,
    TimeSpan Duration,
    double? SummaryBer,
    string? Notes,
    int DataPointCount
);
