using BertBridge.Application.Contracts;
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
}
