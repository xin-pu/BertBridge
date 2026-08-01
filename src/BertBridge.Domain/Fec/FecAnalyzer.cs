namespace BertBridge.Domain.Fec;

/// <summary>
/// FEC 分析器实体。属于 Device 聚合内部。
/// </summary>
public class FecAnalyzer : Shared.BaseEntity
{
    /// <summary>所属芯片/通道组索引</summary>
    public int ChipIndex { get; }

    /// <summary>当前 FEC 配置</summary>
    public FecConfiguration? CurrentConfiguration { get; private set; }

    /// <summary>是否正在分析</summary>
    public bool IsAnalyzing { get; private set; }

    /// <summary>最新 FEC 统计数据</summary>
    public FecStatistics? LatestStatistics { get; private set; }

    internal FecAnalyzer(Guid id, int chipIndex) : base(id)
    {
        ChipIndex = chipIndex;
        IsAnalyzing = false;
    }

    /// <summary>
    /// 启动 FEC 分析。
    /// </summary>
    internal void Start(FecConfiguration configuration)
    {
        CurrentConfiguration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        IsAnalyzing = true;
    }

    /// <summary>
    /// 停止 FEC 分析。
    /// </summary>
    internal void Stop()
    {
        IsAnalyzing = false;
    }

    /// <summary>
    /// 更新统计数据。
    /// </summary>
    internal void UpdateStatistics(FecStatistics statistics)
    {
        LatestStatistics = statistics ?? throw new ArgumentNullException(nameof(statistics));
    }
}
