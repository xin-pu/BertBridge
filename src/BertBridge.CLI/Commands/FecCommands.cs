using System.CommandLine;
using BertBridge.Application.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;

namespace BertBridge.CLI.Commands;

/// <summary>
/// FEC 分析 CLI 命令。
/// </summary>
internal static class FecCommands
{
    public static Command Create(IServiceProvider services)
    {
        var fecCommand = new Command("fec", "FEC 分析命令");

        fecCommand.AddCommand(CreateReadCommand(services));

        return fecCommand;
    }

    private static Command CreateReadCommand(IServiceProvider services)
    {
        var deviceIdArg = new Argument<Guid>("deviceId", "设备 ID");
        var chipIndexArg = new Argument<int>("chipIndex", "芯片索引 (0-based)");

        var cmd = new Command("read", "读取 FEC 统计信息")
        {
            deviceIdArg,
            chipIndexArg
        };

        cmd.SetHandler(async (Guid deviceId, int chipIndex) =>
        {
            using var scope = services.CreateScope();
            var appService = scope.ServiceProvider.GetRequiredService<IFecAppService>();

            var stats = await appService.ReadFecStatisticsAsync(deviceId, chipIndex);

            var table = new Table()
                .Border(TableBorder.Rounded)
                .HideHeaders()
                .AddColumn("Key")
                .AddColumn("Value")
                .AddRow("Pre-FEC BER", stats.PreFecBer.HasValue ? $"{stats.PreFecBer:E2}" : "-")
                .AddRow("Post-FEC BER", stats.PostFecBer.HasValue ? $"{stats.PostFecBer:E2}" : "-")
                .AddRow("可纠错码字数", $"{stats.CorrectableCodewords:N0}")
                .AddRow("不可纠错码字数", $"{stats.UncorrectableCodewords:N0}")
                .AddRow("符号错误数", $"{stats.SymbolErrors:N0}")
                .AddRow("FEC 锁定", stats.IsLocked ? "[green]✓ 已锁定[/]" : "[red]✗ 未锁定[/]")
                .AddRow("时间", stats.Timestamp.ToString("yyyy-MM-dd HH:mm:ss"));

            AnsiConsole.Write(table);
        }, deviceIdArg, chipIndexArg);

        return cmd;
    }
}
