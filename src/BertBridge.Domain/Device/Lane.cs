namespace BertBridge.Domain.Device;

/// <summary>
/// 通道（Lane）实体。属于 Device 聚合的内部实体，
/// 每个 Lane 包含独立的 PG 和 ED 状态快照。
/// </summary>
public class Lane : Shared.BaseEntity
{
    /// <summary>通道索引 (0-based)</summary>
    public int LaneIndex { get; }

    /// <summary>通道名称</summary>
    public string LaneName { get; private set; }

    /// <summary>PG 是否启用</summary>
    public bool PgEnabled { get; private set; }

    /// <summary>ED 是否启用</summary>
    public bool EdEnabled { get; private set; }

    /// <summary>当前 PG 码型名称</summary>
    public string? CurrentPattern { get; private set; }

    internal Lane(Guid id, int laneIndex, string laneName) : base(id)
    {
        if (laneIndex < 0)
            throw new ArgumentException("通道索引不能为负数。", nameof(laneIndex));
        if (string.IsNullOrWhiteSpace(laneName))
            throw new ArgumentException("通道名称不能为空。", nameof(laneName));

        LaneIndex = laneIndex;
        LaneName = laneName;
        PgEnabled = false;
        EdEnabled = false;
    }

    /// <summary>
    /// 启用码型发生器。
    /// </summary>
    internal void EnablePatternGenerator(string pattern)
    {
        if (string.IsNullOrWhiteSpace(pattern))
            throw new ArgumentException("码型名称不能为空。", nameof(pattern));

        PgEnabled = true;
        CurrentPattern = pattern;
    }

    /// <summary>
    /// 禁用码型发生器。
    /// </summary>
    internal void DisablePatternGenerator()
    {
        PgEnabled = false;
    }

    /// <summary>
    /// 启用误码检测器。
    /// </summary>
    internal void EnableErrorDetector()
    {
        EdEnabled = true;
    }

    /// <summary>
    /// 禁用误码检测器。
    /// </summary>
    internal void DisableErrorDetector()
    {
        EdEnabled = false;
    }

    /// <summary>
    /// 更新通道名称。
    /// </summary>
    internal void Rename(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
            throw new ArgumentException("通道名称不能为空。", nameof(newName));
        LaneName = newName;
    }
}
