using BertBridge.Domain.ErrorDetection;
using BertBridge.Domain.Shared;

namespace BertBridge.Domain.TestSession;

/// <summary>
/// 测试会话聚合根，记录单次测试的配置快照、BER 采样和最终结果。
/// </summary>
public class TestSession : BaseAggregateRoot
{
    private Guid _configurationDeviceId;
    private int _configurationLaneCount;
    private string _configurationPatternsJson = string.Empty;
    private TimeSpan? _configurationDuration;
    private DateTime _configurationSnapshotTime;
    private double? _summaryBerMantissa;
    private int? _summaryBerExponent;
    private ulong? _summaryBerErrorCount;
    private ulong? _summaryBerTotalCount;

    private readonly List<BerDataPoint> _dataPoints = [];

    public Guid DeviceId { get; private set; }

    public TestConfiguration Configuration => new(
        _configurationDeviceId,
        _configurationLaneCount,
        _configurationPatternsJson,
        _configurationDuration,
        _configurationSnapshotTime);

    public TestStatus Status { get; private set; }

    public DateTime? StartedAt { get; private set; }

    public DateTime? CompletedAt { get; private set; }

    public TimeSpan Duration => CompletedAt.HasValue && StartedAt.HasValue
        ? CompletedAt.Value - StartedAt.Value
        : (StartedAt.HasValue ? DateTime.UtcNow - StartedAt.Value : TimeSpan.Zero);

    public IReadOnlyList<BerDataPoint> DataPoints => _dataPoints.AsReadOnly();

    public string? Notes { get; private set; }

    public BerValue? SummaryBer =>
        _summaryBerMantissa is null ||
        _summaryBerExponent is null ||
        _summaryBerErrorCount is null ||
        _summaryBerTotalCount is null
            ? null
            : new BerValue(
                _summaryBerMantissa.Value,
                _summaryBerExponent.Value,
                _summaryBerErrorCount.Value,
                _summaryBerTotalCount.Value);

    private TestSession() : base()
    {
        Status = TestStatus.Idle;
    }

    private TestSession(Guid id, Guid deviceId, TestConfiguration configuration) : base(id)
    {
        DeviceId = deviceId;
        SetConfiguration(configuration ?? throw new ArgumentNullException(nameof(configuration)));
        Status = TestStatus.Idle;
    }

    public static TestSession Create(Guid deviceId, TestConfiguration configuration)
    {
        return new TestSession(Guid.NewGuid(), deviceId, configuration);
    }

    public void Start()
    {
        if (Status != TestStatus.Idle)
            throw new InvalidOperationException("只能从 Idle 状态开始测试。");

        Status = TestStatus.Running;
        StartedAt = DateTime.UtcNow;

        RaiseDomainEvent(new Events.TestStartedEvent(Id, DeviceId, Configuration));
    }

    public void AddDataPoint(int laneIndex, ulong errorCount, ulong totalCount, double snr = 0)
    {
        if (Status != TestStatus.Running)
            throw new InvalidOperationException("只能在 Running 状态下添加数据点。");

        var ber = totalCount > 0 ? (double)errorCount / totalCount : 0;
        _dataPoints.Add(new BerDataPoint(
            Guid.NewGuid(),
            Id,
            laneIndex,
            DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            errorCount,
            totalCount,
            ber,
            snr > 0 ? snr : null));
    }

    public void Complete()
    {
        if (Status != TestStatus.Running)
            throw new InvalidOperationException("只能在 Running 状态下完成测试。");

        Status = TestStatus.Completed;
        CompletedAt = DateTime.UtcNow;

        ulong totalErrors = 0;
        ulong totalBits = 0;
        foreach (var dp in _dataPoints)
        {
            totalErrors = Math.Max(totalErrors, dp.ErrorCount);
            totalBits = Math.Max(totalBits, dp.TotalCount);
        }

        SetSummaryBer(BerValue.Calculate(totalErrors, totalBits));
        RaiseDomainEvent(new Events.TestCompletedEvent(Id, DeviceId, SummaryBer!, Duration));
    }

    public void Abort(string reason)
    {
        if (Status != TestStatus.Running)
            throw new InvalidOperationException("只能在 Running 状态下中止测试。");

        Status = TestStatus.Aborted;
        CompletedAt = DateTime.UtcNow;

        RaiseDomainEvent(new Events.TestAbortedEvent(Id, DeviceId, reason, Duration));
    }

    public void AddNotes(string notes)
    {
        Notes = notes;
    }

    private void SetConfiguration(TestConfiguration configuration)
    {
        _configurationDeviceId = configuration.DeviceId;
        _configurationLaneCount = configuration.LaneCount;
        _configurationPatternsJson = configuration.PatternsJson;
        _configurationDuration = configuration.Duration;
        _configurationSnapshotTime = configuration.SnapshotTime;
    }

    private void SetSummaryBer(BerValue summaryBer)
    {
        _summaryBerMantissa = summaryBer.Mantissa;
        _summaryBerExponent = summaryBer.Exponent;
        _summaryBerErrorCount = summaryBer.ErrorCount;
        _summaryBerTotalCount = summaryBer.TotalCount;
    }
}
