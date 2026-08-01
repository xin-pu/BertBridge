using BertBridge.Domain.Shared;

namespace BertBridge.Domain.Fec.Events;

/// <summary>
/// FEC 统计更新领域事件。
/// </summary>
public sealed record FecStatsUpdatedEvent(
    Guid DeviceId,
    int ChipIndex,
    FecStatistics Statistics
) : IDomainEvent;
