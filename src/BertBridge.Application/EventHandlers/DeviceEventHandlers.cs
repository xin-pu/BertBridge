using BertBridge.Domain.Device;
using BertBridge.Domain.Device.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BertBridge.Application.EventHandlers;

/// <summary>
/// 设备连接事件处理。设备连接后执行初始化操作。
/// </summary>
public class DeviceConnectedEventHandler : INotificationHandler<DeviceConnectedEvent>
{
    private readonly ILogger<DeviceConnectedEventHandler> _logger;

    public DeviceConnectedEventHandler(ILogger<DeviceConnectedEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(DeviceConnectedEvent notification, CancellationToken ct)
    {
        _logger.LogInformation("设备 {DeviceId} 已连接: {ConnectionString}",
            notification.DeviceId, notification.ConnectionString);
        return Task.CompletedTask;
    }
}

/// <summary>
/// 设备断开事件处理。
/// </summary>
public class DeviceDisconnectedEventHandler : INotificationHandler<DeviceDisconnectedEvent>
{
    private readonly ILogger<DeviceDisconnectedEventHandler> _logger;

    public DeviceDisconnectedEventHandler(ILogger<DeviceDisconnectedEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(DeviceDisconnectedEvent notification, CancellationToken ct)
    {
        _logger.LogInformation("设备 {DeviceId} 已断开: {ConnectionString}",
            notification.DeviceId, notification.ConnectionString);
        return Task.CompletedTask;
    }
}

/// <summary>
/// 设备状态变更事件处理。
/// </summary>
public class DeviceStateChangedEventHandler : INotificationHandler<DeviceStateChangedEvent>
{
    private readonly ILogger<DeviceStateChangedEventHandler> _logger;

    public DeviceStateChangedEventHandler(ILogger<DeviceStateChangedEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(DeviceStateChangedEvent notification, CancellationToken ct)
    {
        _logger.LogWarning("设备 {DeviceId} 状态变更: {State}, {Message}",
            notification.DeviceId, notification.NewState, notification.Message);
        return Task.CompletedTask;
    }
}
