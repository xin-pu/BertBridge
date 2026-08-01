using System.CommandLine;
using BertBridge.Application.Contracts;
using BertBridge.Application.Dtos;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;

namespace BertBridge.CLI.Commands;

/// <summary>
/// 设备相关 CLI 命令。
/// </summary>
internal static class DeviceCommands
{
    public static Command Create(IServiceProvider services)
    {
        var deviceCommand = new Command("device", "设备管理命令");

        deviceCommand.AddCommand(CreateListCommand(services));
        deviceCommand.AddCommand(CreateConnectCommand(services));
        deviceCommand.AddCommand(CreateDisconnectCommand(services));
        deviceCommand.AddCommand(CreateInfoCommand(services));
        deviceCommand.AddCommand(CreateLanesCommand(services));

        return deviceCommand;
    }

    private static Command CreateListCommand(IServiceProvider services)
    {
        var cmd = new Command("list", "列出所有设备");

        cmd.SetHandler(async () =>
        {
            using var scope = services.CreateScope();
            var appService = scope.ServiceProvider.GetRequiredService<IDeviceAppService>();

            var devices = await appService.GetAllDevicesAsync();

            if (devices.Count == 0)
            {
                AnsiConsole.MarkupLine("[yellow]没有已注册的设备[/]");
                return;
            }

            var table = new Table()
                .Border(TableBorder.Rounded)
                .AddColumn(new TableColumn("[bold]ID[/]").Centered())
                .AddColumn("[bold]名称[/]")
                .AddColumn("[bold]型号[/]")
                .AddColumn("[bold]状态[/]");

            foreach (var d in devices)
            {
                var stateColor = d.ConnectionState == "Connected" ? "green" : "grey";
                table.AddRow(
                    d.Id.ToString("D")[..8] + "...",
                    d.DeviceName,
                    d.Model ?? "-",
                    $"[{stateColor}]{d.ConnectionState}[/]"
                );
            }

            AnsiConsole.Write(table);
        });

        return cmd;
    }

    private static Command CreateConnectCommand(IServiceProvider services)
    {
        var connectionStringArg = new Argument<string>("connectionString", "连接字符串 (如 COM3:115200 或 192.168.1.1:5025)");
        var nameOpt = new Option<string>("--name", () => "Default Device", "设备名称");

        var cmd = new Command("connect", "连接设备")
        {
            connectionStringArg,
            nameOpt
        };

        cmd.SetHandler(async (string connectionString, string name) =>
        {
            using var scope = services.CreateScope();
            var appService = scope.ServiceProvider.GetRequiredService<IDeviceAppService>();

            await AnsiConsole.Status()
                .StartAsync("正在连接设备...", async _ =>
                {
                    var device = await appService.ConnectAsync(connectionString, name);
                    AnsiConsole.MarkupLine($"[green]✓ 设备已连接[/]");
                    AnsiConsole.MarkupLine($"  ID: {device.Id}");
                    AnsiConsole.MarkupLine($"  名称: {device.DeviceName}");
                    AnsiConsole.MarkupLine($"  型号: {device.Model ?? "-"}");
                    AnsiConsole.MarkupLine($"  通道数: {device.LaneCount}");
                });
        }, connectionStringArg, nameOpt);

        return cmd;
    }

    private static Command CreateDisconnectCommand(IServiceProvider services)
    {
        var deviceIdArg = new Argument<Guid>("deviceId", "设备 ID");

        var cmd = new Command("disconnect", "断开设备")
        {
            deviceIdArg
        };

        cmd.SetHandler(async (Guid deviceId) =>
        {
            using var scope = services.CreateScope();
            var appService = scope.ServiceProvider.GetRequiredService<IDeviceAppService>();

            await appService.DisconnectAsync(deviceId);
            AnsiConsole.MarkupLine($"[green]✓ 设备已断开: {deviceId}[/]");
        }, deviceIdArg);

        return cmd;
    }

    private static Command CreateInfoCommand(IServiceProvider services)
    {
        var deviceIdArg = new Argument<Guid>("deviceId", "设备 ID");

        var cmd = new Command("info", "查看设备详情")
        {
            deviceIdArg
        };

        cmd.SetHandler(async (Guid deviceId) =>
        {
            using var scope = services.CreateScope();
            var appService = scope.ServiceProvider.GetRequiredService<IDeviceAppService>();

            var device = await appService.GetDeviceAsync(deviceId);
            if (device == null)
            {
                AnsiConsole.MarkupLine("[red]设备不存在[/]");
                return;
            }

            var panel = new Panel($"""
                [bold]设备详情[/]
                ────────────────
                ID:        {device.Id}
                名称:      {device.DeviceName}
                型号:      {device.Model ?? "-"}
                序列号:    {device.SerialNumber ?? "-"}
                固件版本:  {device.FirmwareVersion ?? "-"}
                连接串:    {device.ConnectionString ?? "-"}
                状态:      {device.ConnectionState}
                通道数:    {device.LaneCount}
                """)
            {
                Border = BoxBorder.Rounded,
                Header = new PanelHeader(" Device Info ")
            };

            AnsiConsole.Write(panel);
        }, deviceIdArg);

        return cmd;
    }

    private static Command CreateLanesCommand(IServiceProvider services)
    {
        var deviceIdArg = new Argument<Guid>("deviceId", "设备 ID");

        var cmd = new Command("lanes", "列出设备通道")
        {
            deviceIdArg
        };

        cmd.SetHandler(async (Guid deviceId) =>
        {
            using var scope = services.CreateScope();
            var appService = scope.ServiceProvider.GetRequiredService<IDeviceAppService>();

            var lanes = await appService.GetLanesAsync(deviceId);

            if (lanes.Count == 0)
            {
                AnsiConsole.MarkupLine("[yellow]没有通道信息[/]");
                return;
            }

            var table = new Table()
                .Border(TableBorder.Rounded)
                .AddColumn(new TableColumn("[bold]通道[/]").Centered())
                .AddColumn("[bold]名称[/]")
                .AddColumn("[bold]PG 状态[/]")
                .AddColumn("[bold]ED 状态[/]")
                .AddColumn("[bold]码型[/]");

            foreach (var lane in lanes)
            {
                var pgState = lane.PgEnabled ? "[green]ON[/]" : "[grey]OFF[/]";
                var edState = lane.EdEnabled ? "[green]ON[/]" : "[grey]OFF[/]";
                table.AddRow(
                    lane.LaneIndex.ToString(),
                    lane.LaneName,
                    pgState,
                    edState,
                    lane.CurrentPattern ?? "-"
                );
            }

            AnsiConsole.Write(table);
        }, deviceIdArg);

        return cmd;
    }
}
