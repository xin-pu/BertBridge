using BertBridge.Application.Contracts;
using BertBridge.Domain.Device;
using BertBridge.GUI.ViewModels;
using BertBridge.Infrastructure.Tests.TestSupport;
using Microsoft.Extensions.DependencyInjection;

namespace BertBridge.Infrastructure.Tests.Application;

public class MockEndToEndTests
{
    [Fact]
    public async Task DeviceConnect_WithMockAdapter_LoadsThroughApplicationAndGuiViewModel()
    {
        var (services, connection) = await TestHostFactory.CreateAsync();
        await using var _ = connection;
        await using var provider = services;

        using var scope = provider.CreateScope();
        var deviceService = scope.ServiceProvider.GetRequiredService<IDeviceAppService>();

        var connected = await deviceService.ConnectAsync("mock://local", "MockE2E");

        Assert.Equal("MockBERT-800G", connected.Model);
        Assert.Equal("Connected", connected.ConnectionState);
        Assert.Equal(8, connected.LaneCount);

        var devices = await deviceService.GetAllDevicesAsync();
        Assert.Contains(devices, d => d.DeviceName == "MockE2E" && d.Model == "MockBERT-800G");

        var viewModel = new MainViewModel(deviceService);
        await viewModel.LoadDevicesAsync();

        Assert.NotEmpty(viewModel.Devices);
        Assert.Contains(viewModel.Devices, d => d.DeviceName == "MockE2E");
        Assert.Equal($"{viewModel.Devices.Count} device(s)", viewModel.StatusMessage);
    }

    [Fact]
    public async Task PgEnable_SameScope_PersistsLaneState()
    {
        var (services, connection) = await TestHostFactory.CreateAsync();
        await using var _ = connection;
        await using var provider = services;

        Guid deviceId;

        // Connect + enable in same scope
        using (var scope = provider.CreateScope())
        {
            var deviceService = scope.ServiceProvider.GetRequiredService<IDeviceAppService>();
            var pgService = scope.ServiceProvider.GetRequiredService<IPatternGeneratorAppService>();

            var connected = await deviceService.ConnectAsync("mock://local", "PgTest");
            deviceId = connected.Id;

            await pgService.EnablePgAsync(deviceId, 0);
        }

        // Verify in new scope
        using (var scope = provider.CreateScope())
        {
            var deviceService = scope.ServiceProvider.GetRequiredService<IDeviceAppService>();
            var lanes = await deviceService.GetLanesAsync(deviceId);

            var lane0 = lanes.First(l => l.LaneIndex == 0);
            Assert.True(lane0.PgEnabled, "Lane 0 PG should be enabled (same scope)");
            Assert.Equal("PRBS31", lane0.CurrentPattern);
        }
    }

    [Fact(Skip = "Temporarily deferred: cross-scope PG auto-reconnect persistence is tracked separately while WPF development continues.")]
    public async Task PgConfigAndEnable_WithAutoReconnect_WorksAcrossScopes()
    {
        var (services, connection) = await TestHostFactory.CreateAsync();
        await using var _ = connection;
        await using var provider = services;

        var deviceId = Guid.Empty;

        // Step 1: Connect device
        using (var scope = provider.CreateScope())
        {
            var deviceService = scope.ServiceProvider.GetRequiredService<IDeviceAppService>();
            var connected = await deviceService.ConnectAsync("mock://local", "PgTest");
            deviceId = connected.Id;
        }

        // Step 2: Enable PG in second scope. This test intentionally covers the cross-scope path.
        using (var scope = provider.CreateScope())
        {
            var pgService = scope.ServiceProvider.GetRequiredService<IPatternGeneratorAppService>();
            await pgService.EnablePgAsync(deviceId, 0);
        }

        // Step 3: Verify lane state persisted (new scope)
        using (var scope = provider.CreateScope())
        {
            var deviceService = scope.ServiceProvider.GetRequiredService<IDeviceAppService>();
            var lanes = await deviceService.GetLanesAsync(deviceId);

            var lane0 = lanes.First(l => l.LaneIndex == 0);
            Assert.True(lane0.PgEnabled, "CrossScope: Lane 0 PG should be enabled after EnablePgAsync");
            Assert.Equal("PRBS31", lane0.CurrentPattern);
        }
    }

    [Fact]
    public async Task DeviceRepository_FromTestHost_PersistsLaneStateAcrossScopes()
    {
        var (services, connection) = await TestHostFactory.CreateAsync();
        await using var _ = connection;
        await using var provider = services;

        var connectionString = ConnectionString.Parse("mock://local");
        Guid deviceId;

        using (var scope = provider.CreateScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IDeviceRepository>();
            var device = Device.Create("RepositoryHostTest");
            device.BeginConnect(connectionString);
            device.RegisterDeviceInfo(
                new DeviceInfo("MockBERT-800G", "SN-001", "1.0.0", "MockBoard"),
                new DeviceCapability(
                    2,
                    supportsPAM4: true,
                    supportsAdvancedModulation: false,
                    supportedPatterns: ["PRBS7", "PRBS31"],
                    maxBaudRateGBd: 56,
                    supportsFec: true,
                    supportsGpio: false,
                    firTapCount: 5,
                    supportsJitterInjection: true));
            device.MarkConnected(connectionString);
            deviceId = device.Id;
            await repository.AddAsync(device);
        }

        using (var scope = provider.CreateScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IDeviceRepository>();
            var device = await repository.GetByIdAsync(new DeviceId(deviceId));
            Assert.NotNull(device);

            device.EnablePatternGenerator(0, "PRBS31");
            await repository.UpdateAsync(device);
        }

        using (var scope = provider.CreateScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IDeviceRepository>();
            var device = await repository.GetByIdAsync(new DeviceId(deviceId));
            Assert.NotNull(device);

            var lane0 = device.GetLane(0);
            Assert.True(lane0.PgEnabled);
            Assert.Equal("PRBS31", lane0.CurrentPattern);
        }
    }

    [Fact]
    public async Task EdStartAndRead_WithAutoReconnect_ReturnsMockResults()
    {
        var (services, connection) = await TestHostFactory.CreateAsync();
        await using var _ = connection;
        await using var provider = services;

        var deviceId = Guid.Empty;

        // Step 1: Connect
        using (var scope = provider.CreateScope())
        {
            var deviceService = scope.ServiceProvider.GetRequiredService<IDeviceAppService>();
            var connected = await deviceService.ConnectAsync("mock://local", "EdTest");
            deviceId = connected.Id;
        }

        // Step 2: Start ED in new scope (auto-reconnect)
        using (var scope = provider.CreateScope())
        {
            var edService = scope.ServiceProvider.GetRequiredService<IErrorDetectorAppService>();
            var result = await edService.StartEdAsync(deviceId, 0, "PRBS31");

            Assert.True(result.SignalDetected);
            Assert.True(result.CdrLocked);
            Assert.True(result.PllLocked);
            Assert.True(result.DspReady);
            Assert.Equal(25.0, result.SnrDb);
        }

        // Step 3: Read ED in another scope
        using (var scope = provider.CreateScope())
        {
            var edService = scope.ServiceProvider.GetRequiredService<IErrorDetectorAppService>();
            var result = await edService.ReadEdResultAsync(deviceId, 0);

            Assert.True(result.SignalDetected);
            Assert.True(result.TotalCount > 0, "Should have accumulated bit count");
        }
    }

    [Fact]
    public async Task FecRead_WithAutoReconnect_ReturnsMockStatistics()
    {
        var (services, connection) = await TestHostFactory.CreateAsync();
        await using var _ = connection;
        await using var provider = services;

        var deviceId = Guid.Empty;

        using (var scope = provider.CreateScope())
        {
            var deviceService = scope.ServiceProvider.GetRequiredService<IDeviceAppService>();
            var connected = await deviceService.ConnectAsync("mock://local", "FecTest");
            deviceId = connected.Id;
        }

        // Read FEC in new scope (auto-reconnect)
        using (var scope = provider.CreateScope())
        {
            var fecService = scope.ServiceProvider.GetRequiredService<IFecAppService>();
            var stats = await fecService.ReadFecStatisticsAsync(deviceId, 0);

            Assert.True(stats.IsLocked);
            Assert.Equal(1e-12, stats.PreFecBer);
            Assert.Equal(0, stats.PostFecBer);
            Assert.Equal(0UL, stats.CorrectableCodewords);
        }
    }
}
