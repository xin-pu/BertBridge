namespace BertBridge.Domain.TestSession;

/// <summary>
/// 测试配置快照值对象。记录测试开始时的完整配置状态。
/// </summary>
public sealed class TestConfiguration : Shared.ValueObject
{
    /// <summary>关联设备 ID</summary>
    public Guid DeviceId { get; }

    /// <summary>通道数量</summary>
    public int LaneCount { get; }

    /// <summary>每通道码型配置（JSON 序列化存储）</summary>
    public string PatternsJson { get; }

    /// <summary>测试时长限制（null 表示无限制/连续测试）</summary>
    public TimeSpan? Duration { get; }

    /// <summary>配置快照时间戳</summary>
    public DateTime SnapshotTime { get; }

    private TestConfiguration()
    {
        PatternsJson = string.Empty;
    }

    public TestConfiguration(Guid deviceId, int laneCount, string patternsJson, TimeSpan? duration)
    {
        if (deviceId == Guid.Empty)
            throw new ArgumentException("设备 ID 不能为空。", nameof(deviceId));
        if (laneCount <= 0)
            throw new ArgumentException("通道数必须大于零。", nameof(laneCount));

        DeviceId = deviceId;
        LaneCount = laneCount;
        PatternsJson = patternsJson ?? throw new ArgumentNullException(nameof(patternsJson));
        Duration = duration;
        SnapshotTime = DateTime.UtcNow;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return DeviceId;
        yield return LaneCount;
        yield return PatternsJson;
        yield return SnapshotTime;
    }
}
