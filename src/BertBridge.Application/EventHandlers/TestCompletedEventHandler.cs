using BertBridge.Domain.TestSession.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BertBridge.Application.EventHandlers;

/// <summary>
/// 测试完成事件处理。
/// </summary>
public class TestCompletedEventHandler : INotificationHandler<TestCompletedEvent>
{
    private readonly ILogger<TestCompletedEventHandler> _logger;

    public TestCompletedEventHandler(ILogger<TestCompletedEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(TestCompletedEvent notification, CancellationToken ct)
    {
        _logger.LogInformation("测试完成: SessionId={SessionId}, DeviceId={DeviceId}, BER={Ber}, Duration={Duration}",
            notification.TestSessionId, notification.DeviceId,
            notification.SummaryBer?.ToString() ?? "N/A", notification.Duration);
        return Task.CompletedTask;
    }
}
