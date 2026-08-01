namespace BertBridge.Domain.PatternGeneration;

/// <summary>
/// FIR/FFE 抽头系数值对象。用于前馈均衡配置。
/// </summary>
public sealed class FirTaps : Shared.ValueObject
{
    /// <summary>Pre-cursor tap 系数</summary>
    public decimal PreCursor { get; }

    /// <summary>Main cursor tap 系数</summary>
    public decimal MainCursor { get; }

    /// <summary>Post-cursor tap 系数列表（Post1..PostN）</summary>
    public IReadOnlyList<decimal> PostCursors { get; }

    public FirTaps(decimal preCursor, decimal mainCursor, params decimal[] postCursors)
    {
        PreCursor = preCursor;
        MainCursor = mainCursor;
        PostCursors = postCursors.ToList().AsReadOnly();
    }

    /// <summary>总抽头数（含 Pre、Main、Post）</summary>
    public int TapCount => 1 + 1 + PostCursors.Count;

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return PreCursor;
        yield return MainCursor;
        foreach (var tap in PostCursors)
            yield return tap;
    }

    public override string ToString()
        => $"Pre={PreCursor}, Main={MainCursor}, Post=[{string.Join(", ", PostCursors)}]";
}
