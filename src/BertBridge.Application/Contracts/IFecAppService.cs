using BertBridge.Application.Dtos;

namespace BertBridge.Application.Contracts;

/// <summary>
/// FEC 应用服务接口。
/// </summary>
public interface IFecAppService
{
    Task<FecStatisticsDto> ReadFecStatisticsAsync(Guid deviceId, int chipIndex, CancellationToken ct = default);
}
