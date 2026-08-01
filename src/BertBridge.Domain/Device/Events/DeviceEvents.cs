using BertBridge.Domain.Shared;

namespace BertBridge.Domain.Device.Events;

/// <summary>
/// 设备已连接领域事件。
/// </summary>
public sealed record DeviceConnectedEvent(
    DeviceId DeviceId,
    ConnectionString ConnectionString
) : IDomainEvent;

/// <summary>
/// 设备已断开领域事件。
/// </summary>
public sealed record DeviceDisconnectedEvent(
    DeviceId DeviceId,
    ConnectionString ConnectionString
) : IDomainEvent;

/// <summary>
/// 设备状态变更领域事件。
/// </summary>
public sealed record DeviceStateChangedEvent(
    DeviceId DeviceId,
    ConnectionState NewState,
    string? Message
) : IDomainEvent;
