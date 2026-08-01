using BertBridge.Domain.ErrorDetection.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BertBridge.Application.EventHandlers;

/// <summary>
/// BER 超阈值事件处理。
/// </summary>
public class BerThresholdExceededEventHandler : INotificationHandler<BerThresholdExceededEvent>
{
    private readonly ILogger<BerThresholdExceededEventHandler> _logger;

    public BerThresholdExceededEventHandler(ILogger<BerThresholdExceededEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(BerThresholdExceededEvent notification, CancellationToken ct)
    {
        _logger.LogWarning("BER 超阈值! 设备={DeviceId}, Lane={LaneIndex}, 当前BER={Ber}, 阈值={Threshold}",
            notification.DeviceId, notification.LaneIndex,
            notification.CurrentBer, notification.Threshold);
        return Task.CompletedTask;
    }
}
