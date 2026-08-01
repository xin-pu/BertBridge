using MediatR;

namespace BertBridge.Domain.Shared;

/// <summary>
/// 领域事件标记接口。所有领域事件必须实现此接口，
/// 通过 MediatR 的 INotification 机制分发。
/// </summary>
public interface IDomainEvent : INotification
{
}
