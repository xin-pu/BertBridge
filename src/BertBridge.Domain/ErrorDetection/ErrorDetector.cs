using BertBridge.Domain.PatternGeneration;

namespace BertBridge.Domain.ErrorDetection;

/// <summary>
/// 误码检测器实体。属于 Device 聚合内部，管理单通道 ED 生命周期。
/// </summary>
public class ErrorDetector : Shared.BaseEntity
{
    /// <summary>所属通道索引</summary>
    public int LaneIndex { get; }

    /// <summary>是否正在检测</summary>
    public bool IsRunning { get; private set; }

    /// <summary>当前配置</summary>
    public EdConfiguration? CurrentConfiguration { get; private set; }

    /// <summary>最后一次读取的结果</summary>
    public EdResult? LastResult { get; private set; }

    internal ErrorDetector(Guid id, int laneIndex) : base(id)
    {
        LaneIndex = laneIndex;
        IsRunning = false;
    }

    /// <summary>
    /// 启动误码检测。
    /// </summary>
    internal void Start(EdConfiguration configuration)
    {
        CurrentConfiguration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        IsRunning = true;
    }

    /// <summary>
    /// 停止误码检测。
    /// </summary>
    internal void Stop()
    {
        IsRunning = false;
    }

    /// <summary>
    /// 更新检测结果。
    /// </summary>
    internal void UpdateResult(EdResult result)
    {
        LastResult = result ?? throw new ArgumentNullException(nameof(result));
    }

    /// <summary>
    /// 检查是否超过 BER 阈值。
    /// </summary>
    internal bool IsBerThresholdExceeded()
    {
        if (CurrentConfiguration?.BerThreshold == null || LastResult == null)
            return false;

        // 比较指数：指数越负表示 BER 越低（越好），超过阈值意味着 BER 更大
        return LastResult.Ber.Exponent > CurrentConfiguration.BerThreshold.Exponent
            || (LastResult.Ber.Exponent == CurrentConfiguration.BerThreshold.Exponent
                && LastResult.Ber.Mantissa > CurrentConfiguration.BerThreshold.Mantissa);
    }
}
