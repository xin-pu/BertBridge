using System.Collections.ObjectModel;
using BertBridge.Application.Contracts;
using BertBridge.Application.Dtos;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BertBridge.GUI.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly IDeviceAppService _deviceAppService;

    [ObservableProperty]
    private ObservableCollection<DeviceListItemDto> devices = [];

    [ObservableProperty]
    private string statusMessage = "Ready";

    public MainViewModel(IDeviceAppService deviceAppService)
    {
        _deviceAppService = deviceAppService;
    }

    [RelayCommand]
    public async Task LoadDevicesAsync()
    {
        var items = await _deviceAppService.GetAllDevicesAsync();
        Devices = new ObservableCollection<DeviceListItemDto>(items);
        StatusMessage = $"{Devices.Count} device(s)";
    }
}
