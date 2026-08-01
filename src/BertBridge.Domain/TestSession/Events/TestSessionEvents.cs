using BertBridge.Domain.Shared;

namespace BertBridge.Domain.TestSession.Events;

/// <summary>
/// 测试已开始领域事件。
/// </summary>
public sealed record TestStartedEvent(
    Guid TestSessionId,
    Guid DeviceId,
    TestConfiguration Configuration
) : IDomainEvent;

/// <summary>
/// 测试已完成领域事件。
/// </summary>
public sealed record TestCompletedEvent(
    Guid TestSessionId,
    Guid DeviceId,
    ErrorDetection.BerValue? SummaryBer,
    TimeSpan Duration
) : IDomainEvent;

/// <summary>
/// 测试已中止领域事件。
/// </summary>
public sealed record TestAbortedEvent(
    Guid TestSessionId,
    Guid DeviceId,
    string Reason,
    TimeSpan Duration
) : IDomainEvent;
