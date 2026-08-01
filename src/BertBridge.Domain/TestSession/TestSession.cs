using BertBridge.Domain.Shared;

namespace BertBridge.Domain.TestSession;

/// <summary>
/// 测试会话聚合根。管理单次测试的完整生命周期，
/// 记录配置快照、时序 BER 数据点和最终结果。
/// </summary>
public class TestSession : BaseAggregateRoot
{
    /// <summary>所属设备 ID</summary>
    public Guid DeviceId { get; }

    /// <summary>测试开始时的配置快照</summary>
    public TestConfiguration Configuration { get; }

    /// <summary>测试状态</summary>
    public TestStatus Status { get; private set; }

    /// <summary>开始时间</summary>
    public DateTime? StartedAt { get; private set; }

    /// <summary>完成/中止时间</summary>
    public DateTime? CompletedAt { get; private set; }

    /// <summary>测试时长</summary>
    public TimeSpan Duration => CompletedAt.HasValue && StartedAt.HasValue
        ? CompletedAt.Value - StartedAt.Value
        : (StartedAt.HasValue ? DateTime.UtcNow - StartedAt.Value : TimeSpan.Zero);

    /// <summary>BER 采样数据点列表</summary>
    private readonly List<BerDataPoint> _dataPoints = [];
    public IReadOnlyList<BerDataPoint> DataPoints => _dataPoints.AsReadOnly();

    /// <summary>测试备注</summary>
    public string? Notes { get; private set; }

    /// <summary>汇总 BER</summary>
    public ErrorDetection.BerValue? SummaryBer { get; private set; }

    private TestSession(Guid id, Guid deviceId, TestConfiguration configuration) : base(id)
    {
        DeviceId = deviceId;
        Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        Status = TestStatus.Idle;
    }

    /// <summary>
    /// 创建新的测试会话。
    /// </summary>
    public static TestSession Create(Guid deviceId, TestConfiguration configuration)
    {
        return new TestSession(Guid.NewGuid(), deviceId, configuration);
    }

    /// <summary>
    /// 开始测试。
    /// </summary>
    public void Start()
    {
        if (Status != TestStatus.Idle)
            throw new InvalidOperationException("只能从 Idle 状态开始测试。");

        Status = TestStatus.Running;
        StartedAt = DateTime.UtcNow;

        RaiseDomainEvent(new Events.TestStartedEvent(Id, DeviceId, Configuration));
    }

    /// <summary>
    /// 添加 BER 采样数据点。
    /// </summary>
    public void AddDataPoint(int laneIndex, ulong errorCount, ulong totalCount, double snr = 0)
    {
        if (Status != TestStatus.Running)
            throw new InvalidOperationException("只能在 Running 状态下添加数据点。");

        double ber = totalCount > 0 ? (double)errorCount / totalCount : 0;
        _dataPoints.Add(new BerDataPoint(Guid.NewGuid(), Id, laneIndex,
            DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            errorCount, totalCount, ber, snr > 0 ? snr : null));
    }

    /// <summary>
    /// 完成测试。
    /// </summary>
    public void Complete()
    {
        if (Status != TestStatus.Running)
            throw new InvalidOperationException("只能在 Running 状态下完成测试。");

        Status = TestStatus.Completed;
        CompletedAt = DateTime.UtcNow;

        // 计算汇总 BER
        ulong totalErrors = 0, totalBits = 0;
        foreach (var dp in _dataPoints)
        {
            totalErrors = Math.Max(totalErrors, dp.ErrorCount);
            totalBits = Math.Max(totalBits, dp.TotalCount);
        }
        SummaryBer = ErrorDetection.BerValue.Calculate(totalErrors, totalBits);

        RaiseDomainEvent(new Events.TestCompletedEvent(Id, DeviceId, SummaryBer, Duration));
    }

    /// <summary>
    /// 中止测试。
    /// </summary>
    public void Abort(string reason)
    {
        if (Status != TestStatus.Running)
            throw new InvalidOperationException("只能在 Running 状态下中止测试。");

        Status = TestStatus.Aborted;
        CompletedAt = DateTime.UtcNow;

        RaiseDomainEvent(new Events.TestAbortedEvent(Id, DeviceId, reason, Duration));
    }

    /// <summary>
    /// 添加备注。
    /// </summary>
    public void AddNotes(string notes)
    {
        Notes = notes;
    }
}
