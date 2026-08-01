using Microsoft.EntityFrameworkCore;

namespace BertBridge.Infrastructure.Persistence;

public static class BertBridgeDbContextSchemaExtensions
{
    public static async Task EnsureSchemaAsync(this BertBridgeDbContext dbContext, CancellationToken ct = default)
    {
        await dbContext.Database.EnsureCreatedAsync(ct);

        if (!dbContext.Database.IsSqlite())
            return;

        var existingColumns = await GetDeviceColumnsAsync(dbContext, ct);

        await AddColumnIfMissingAsync(dbContext, existingColumns, "InfoModel", "TEXT", ct);
        await AddColumnIfMissingAsync(dbContext, existingColumns, "InfoSerialNumber", "TEXT", ct);
        await AddColumnIfMissingAsync(dbContext, existingColumns, "InfoFirmwareVersion", "TEXT", ct);
        await AddColumnIfMissingAsync(dbContext, existingColumns, "InfoBoardType", "TEXT", ct);
        await AddColumnIfMissingAsync(dbContext, existingColumns, "ConnectionValue", "TEXT", ct);
        await AddColumnIfMissingAsync(dbContext, existingColumns, "ConnectionProtocol", "TEXT", ct);
        await AddColumnIfMissingAsync(dbContext, existingColumns, "CapabilityMaxLanes", "INTEGER", ct);
        await AddColumnIfMissingAsync(dbContext, existingColumns, "CapabilitySupportsPAM4", "INTEGER", ct);
        await AddColumnIfMissingAsync(dbContext, existingColumns, "CapabilitySupportsAdvancedModulation", "INTEGER", ct);
        await AddColumnIfMissingAsync(dbContext, existingColumns, "CapabilitySupportedPatternsJson", "TEXT", ct);
        await AddColumnIfMissingAsync(dbContext, existingColumns, "CapabilityMaxBaudRateGBd", "TEXT", ct);
        await AddColumnIfMissingAsync(dbContext, existingColumns, "CapabilitySupportsFec", "INTEGER", ct);
        await AddColumnIfMissingAsync(dbContext, existingColumns, "CapabilitySupportsGpio", "INTEGER", ct);
        await AddColumnIfMissingAsync(dbContext, existingColumns, "CapabilityFirTapCount", "INTEGER", ct);
        await AddColumnIfMissingAsync(dbContext, existingColumns, "CapabilitySupportsJitterInjection", "INTEGER", ct);
    }

    private static async Task<HashSet<string>> GetDeviceColumnsAsync(BertBridgeDbContext dbContext, CancellationToken ct)
    {
        var connection = dbContext.Database.GetDbConnection();
        await using var command = connection.CreateCommand();
        command.CommandText = "PRAGMA table_info('Devices');";

        if (connection.State != System.Data.ConnectionState.Open)
            await connection.OpenAsync(ct);

        var columns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        await using var reader = await command.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            columns.Add(reader.GetString(1));
        }

        return columns;
    }

    private static async Task AddColumnIfMissingAsync(
        BertBridgeDbContext dbContext,
        HashSet<string> existingColumns,
        string columnName,
        string columnType,
        CancellationToken ct)
    {
        if (existingColumns.Contains(columnName))
            return;

        var connection = dbContext.Database.GetDbConnection();
        await using var command = connection.CreateCommand();
        command.CommandText = $"ALTER TABLE Devices ADD COLUMN {columnName} {columnType};";

        if (connection.State != System.Data.ConnectionState.Open)
            await connection.OpenAsync(ct);

        await command.ExecuteNonQueryAsync(ct);
        existingColumns.Add(columnName);
    }
}
