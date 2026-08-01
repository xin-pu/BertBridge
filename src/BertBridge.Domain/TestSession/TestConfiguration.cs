namespace BertBridge.Domain.TestSession;

/// <summary>
/// 测试配置快照值对象，记录测试开始时的完整配置状态。
/// </summary>
public sealed class TestConfiguration : Shared.ValueObject
{
    public Guid DeviceId { get; }
    public int LaneCount { get; }
    public string PatternsJson { get; }
    public TimeSpan? Duration { get; }
    public DateTime SnapshotTime { get; }

    public TestConfiguration(Guid deviceId, int laneCount, string patternsJson, TimeSpan? duration)
        : this(deviceId, laneCount, patternsJson, duration, DateTime.UtcNow)
    {
    }

    internal TestConfiguration(Guid deviceId, int laneCount, string patternsJson, TimeSpan? duration, DateTime snapshotTime)
    {
        if (deviceId == Guid.Empty)
            throw new ArgumentException("设备 ID 不能为空。", nameof(deviceId));
        if (laneCount <= 0)
            throw new ArgumentException("通道数必须大于零。", nameof(laneCount));

        DeviceId = deviceId;
        LaneCount = laneCount;
        PatternsJson = patternsJson ?? throw new ArgumentNullException(nameof(patternsJson));
        Duration = duration;
        SnapshotTime = snapshotTime;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return DeviceId;
        yield return LaneCount;
        yield return PatternsJson;
        yield return SnapshotTime;
    }
}
