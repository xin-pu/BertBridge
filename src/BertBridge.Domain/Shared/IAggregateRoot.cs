namespace BertBridge.Domain.Shared;

/// <summary>
/// 聚合根标记接口。用于约束仓储仅接受聚合根类型，
/// 以及 DbContext 拦截 SaveChanges 时识别聚合根。
/// </summary>
public interface IAggregateRoot
{
    IReadOnlyList<IDomainEvent> DomainEvents { get; }

    void ClearDomainEvents();
}
