namespace BertBridge.Domain.Shared;

/// <summary>
/// 实体基类。实体拥有唯一标识 Id，通过 Id 判定相等性。
/// 不持有领域事件（由聚合根管理）。
/// </summary>
public abstract class BaseEntity : IEquatable<BaseEntity>
{
    /// <summary>
    /// 实体唯一标识。
    /// </summary>
    public Guid Id { get; protected set; }

    protected BaseEntity() : this(Guid.NewGuid()) { }

    protected BaseEntity(Guid id)
    {
        Id = id;
    }

    public bool Equals(BaseEntity? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        if (GetType() != other.GetType()) return false;

        return Id == other.Id;
    }

    public override bool Equals(object? obj)
        => Equals(obj as BaseEntity);

    public override int GetHashCode()
        => Id.GetHashCode();

    public static bool operator ==(BaseEntity? left, BaseEntity? right)
    {
        if (left is null && right is null) return true;
        if (left is null || right is null) return false;

        return left.Equals(right);
    }

    public static bool operator !=(BaseEntity? left, BaseEntity? right)
        => !(left == right);
}
