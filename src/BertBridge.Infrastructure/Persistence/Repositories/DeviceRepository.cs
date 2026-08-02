using BertBridge.Domain.Device;
using Microsoft.EntityFrameworkCore;

namespace BertBridge.Infrastructure.Persistence.Repositories;

/// <summary>
/// Device 仓储实现。
/// </summary>
public class DeviceRepository : IDeviceRepository
{
    private readonly BertBridgeDbContext _context;

    public DeviceRepository(BertBridgeDbContext context)
    {
        _context = context;
    }

    public async Task<Device?> GetByIdAsync(DeviceId id, CancellationToken ct = default)
        => await _context.Devices
            .Include(d => d.Lanes)
            .FirstOrDefaultAsync(d => d.Id == id.Value, ct);

    public async Task<Device?> GetByConnectionStringAsync(ConnectionString cs, CancellationToken ct = default)
        => await _context.Devices
            .FirstOrDefaultAsync(d => EF.Property<string?>(d, "_connectionValue") == cs.Value, ct);

    public async Task<IReadOnlyList<Device>> GetAllAsync(CancellationToken ct = default)
        => await _context.Devices
            .OrderBy(d => d.DeviceName)
            .ToListAsync(ct);

    public async Task AddAsync(Device device, CancellationToken ct = default)
    {
        await _context.Devices.AddAsync(device, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Device device, CancellationToken ct = default)
    {
        var laneSnapshots = device.Lanes
            .Select(l => new
            {
                l.Id,
                l.LaneName,
                l.PgEnabled,
                l.EdEnabled,
                l.CurrentPattern
            })
            .ToList();

        _context.Devices.Update(device);
        await _context.SaveChangesAsync(ct);

        foreach (var lane in laneSnapshots)
        {
            await _context.Lanes
                .Where(l => l.Id == lane.Id)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(l => l.LaneName, lane.LaneName)
                    .SetProperty(l => l.PgEnabled, lane.PgEnabled)
                    .SetProperty(l => l.EdEnabled, lane.EdEnabled)
                    .SetProperty(l => l.CurrentPattern, lane.CurrentPattern), ct);
        }
    }

    public async Task DeleteAsync(DeviceId id, CancellationToken ct = default)
    {
        var device = await GetByIdAsync(id, ct);
        if (device != null)
        {
            _context.Devices.Remove(device);
            await _context.SaveChangesAsync(ct);
        }
    }
}
