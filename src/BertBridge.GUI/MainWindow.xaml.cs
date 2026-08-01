using System.Windows;
using BertBridge.GUI.ViewModels;

namespace BertBridge.GUI;

public partial class MainWindow : Window
{
    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        Loaded += async (_, _) => await viewModel.LoadDevicesAsync();
    }
}
