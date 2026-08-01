namespace BertBridge.Domain.Shared;

/// <summary>
/// 聚合根基类。继承 BaseEntity，管理领域事件集合。
/// 子类通过 RaiseDomainEvent() 方法注册领域事件，
/// DbContext 在 SaveChanges 时统一读取并分发。
/// </summary>
public abstract class BaseAggregateRoot : BaseEntity, IAggregateRoot
{
    private readonly List<IDomainEvent> _domainEvents = [];

    protected BaseAggregateRoot() : base() { }

    protected BaseAggregateRoot(Guid id) : base(id) { }

    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>
    /// 注册领域事件。该事件将在 DbContext SaveChanges 时由
    /// DomainEventDispatcher 统一分发。
    /// </summary>
    protected void RaiseDomainEvent(IDomainEvent domainEvent)
        => _domainEvents.Add(domainEvent);

    /// <summary>
    /// 清除所有已注册的领域事件（在分发完成后由基础设施层调用）。
    /// </summary>
    public void ClearDomainEvents()
        => _domainEvents.Clear();
}
