using BertBridge.Domain.Shared;

namespace BertBridge.Domain.PatternGeneration.Events;

/// <summary>
/// PG 输出状态变更领域事件。
/// </summary>
public sealed record PgOutputChangedEvent(
    Guid DeviceId,
    int LaneIndex,
    bool IsOutputting,
    PrbsPattern? Pattern
) : IDomainEvent;
