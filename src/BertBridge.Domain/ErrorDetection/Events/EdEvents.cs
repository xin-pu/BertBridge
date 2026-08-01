using BertBridge.Domain.Shared;

namespace BertBridge.Domain.ErrorDetection.Events;

/// <summary>
/// ED 已启动领域事件。
/// </summary>
public sealed record EdStartedEvent(
    Guid DeviceId,
    int LaneIndex,
    PatternGeneration.PrbsPattern ExpectedPattern
) : IDomainEvent;

/// <summary>
/// ED 已停止领域事件。
/// </summary>
public sealed record EdStoppedEvent(
    Guid DeviceId,
    int LaneIndex,
    BerValue? FinalBer
) : IDomainEvent;

/// <summary>
/// BER 超过阈值告警领域事件。
/// </summary>
public sealed record BerThresholdExceededEvent(
    Guid DeviceId,
    int LaneIndex,
    BerValue CurrentBer,
    BerValue Threshold
) : IDomainEvent;
