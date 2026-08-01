namespace BertBridge.Domain.PatternGeneration;

/// <summary>
/// PG 配置值对象。封装码型发生器的全部配置参数。
/// </summary>
public sealed class PgConfiguration : Shared.ValueObject
{
    /// <summary>PRBS 码型</summary>
    public PrbsPattern Pattern { get; }

    /// <summary>码型模式</summary>
    public PatternMode Mode { get; }

    /// <summary>自定义固定码型（当 Pattern == Custom 时有效）</summary>
    public FixedPattern? FixedPattern { get; }

    /// <summary>PAM4 MSB 码型（当 Mode == MsbLsbIndependent 时有效）</summary>
    public PrbsPattern? MsbPattern { get; }

    /// <summary>PAM4 LSB 码型（当 Mode == MsbLsbIndependent 时有效）</summary>
    public PrbsPattern? LsbPattern { get; }

    /// <summary>FIR 抽头系数</summary>
    public FirTaps? FirTaps { get; }

    /// <summary>输出摆幅</summary>
    public TxSwing? Swing { get; }

    /// <summary>是否启用 Gray 编码（PAM4）</summary>
    public bool GrayEncoding { get; }

    /// <summary>是否极性反转</summary>
    public bool PolarityInvert { get; }

    /// <summary>是否启用预编码</summary>
    public bool PreCoding { get; }

    public PgConfiguration(
        PrbsPattern pattern,
        PatternMode mode = PatternMode.SingleStream,
        FixedPattern? fixedPattern = null,
        PrbsPattern? msbPattern = null,
        PrbsPattern? lsbPattern = null,
        FirTaps? firTaps = null,
        TxSwing? swing = null,
        bool grayEncoding = false,
        bool polarityInvert = false,
        bool preCoding = false)
    {
        Pattern = pattern;
        Mode = mode;
        FixedPattern = fixedPattern;
        MsbPattern = msbPattern;
        LsbPattern = lsbPattern;
        FirTaps = firTaps;
        Swing = swing;
        GrayEncoding = grayEncoding;
        PolarityInvert = polarityInvert;
        PreCoding = preCoding;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Pattern;
        yield return Mode;
        yield return GrayEncoding;
        yield return PolarityInvert;
        yield return PreCoding;
        if (FixedPattern != null) yield return FixedPattern;
        if (FirTaps != null) yield return FirTaps;
        if (Swing != null) yield return Swing;
    }
}
