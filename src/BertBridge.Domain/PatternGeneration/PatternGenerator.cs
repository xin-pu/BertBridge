namespace BertBridge.Domain.PatternGeneration;

/// <summary>
/// 码型发生器实体。属于 Device 聚合内部，记录当前 PG 运行状态。
/// </summary>
public class PatternGenerator : Shared.BaseEntity
{
    /// <summary>所属通道索引</summary>
    public int LaneIndex { get; }

    /// <summary>是否正在输出</summary>
    public bool IsOutputting { get; private set; }

    /// <summary>当前配置</summary>
    public PgConfiguration? CurrentConfiguration { get; private set; }

    internal PatternGenerator(Guid id, int laneIndex) : base(id)
    {
        LaneIndex = laneIndex;
        IsOutputting = false;
    }

    /// <summary>
    /// 应用新配置并开始输出。
    /// </summary>
    internal void Start(PgConfiguration configuration)
    {
        CurrentConfiguration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        IsOutputting = true;
    }

    /// <summary>
    /// 停止输出。
    /// </summary>
    internal void Stop()
    {
        IsOutputting = false;
    }

    /// <summary>
    /// 更新配置但不改变输出状态。
    /// </summary>
    internal void UpdateConfiguration(PgConfiguration configuration)
    {
        CurrentConfiguration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }
}
