namespace BertBridge.PluginSDK;

/// <summary>
/// PG 配置记录。
/// </summary>
public sealed record PgConfiguration(
    string Pattern,
    string Mode = "SingleStream",
    string? CustomPattern = null,
    string? MsbPattern = null,
    string? LsbPattern = null,
    decimal[]? FirTaps = null,
    int? SwingMillivolts = null,
    bool GrayEncoding = false,
    bool PolarityInvert = false,
    bool PreCoding = false
);
