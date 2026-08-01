using BertBridge.Application.Dtos;

namespace BertBridge.Application.Contracts;

/// <summary>
/// PG 应用服务接口。
/// </summary>
public interface IPatternGeneratorAppService
{
    Task ConfigurePgAsync(Guid deviceId, int laneIndex, PgConfigurationDto config, CancellationToken ct = default);
    Task EnablePgAsync(Guid deviceId, int laneIndex, CancellationToken ct = default);
    Task DisablePgAsync(Guid deviceId, int laneIndex, CancellationToken ct = default);
}
