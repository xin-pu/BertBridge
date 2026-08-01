using BertBridge.Application.Dtos;

namespace BertBridge.Application.Contracts;

/// <summary>
/// ED 应用服务接口。
/// </summary>
public interface IErrorDetectorAppService
{
    Task<EdResultDto> StartEdAsync(Guid deviceId, int laneIndex, string expectedPattern, CancellationToken ct = default);
    Task<EdResultDto> StopEdAsync(Guid deviceId, int laneIndex, CancellationToken ct = default);
    Task<EdResultDto> ReadEdResultAsync(Guid deviceId, int laneIndex, CancellationToken ct = default);
}
