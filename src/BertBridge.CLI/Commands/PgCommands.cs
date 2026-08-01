using System.CommandLine;
using System.CommandLine.Invocation;
using BertBridge.Application.Contracts;
using BertBridge.Application.Dtos;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;

namespace BertBridge.CLI.Commands;

/// <summary>
/// PG（码型发生器）CLI 命令。
/// </summary>
internal static class PgCommands
{
    public static Command Create(IServiceProvider services)
    {
        var pgCommand = new Command("pg", "码型发生器 (PG) 管理命令");

        pgCommand.AddCommand(CreateConfigCommand(services));
        pgCommand.AddCommand(CreateEnableCommand(services));
        pgCommand.AddCommand(CreateDisableCommand(services));
        pgCommand.AddCommand(CreateStatusCommand(services));

        return pgCommand;
    }

    private static Command CreateConfigCommand(IServiceProvider services)
    {
        var deviceIdArg = new Argument<Guid>("deviceId", "设备 ID");
        var laneIndexArg = new Argument<int>("laneIndex", "通道索引 (0-7)");

        var patternOpt = new Option<string>("--pattern", () => "PRBS31", "PRBS 码型 (PRBS7/9/11/13/15/20/23/31)");
        var modeOpt = new Option<string>("--mode", () => "SingleStream", "码型模式 (SingleStream/MSBLSB/Sequencer)");
        var customOpt = new Option<string?>("--custom", () => null, "自定义码型 (十六进制)");
        var swingOpt = new Option<int?>("--swing", () => null, "输出摆幅 (mV)");
        var grayOpt = new Option<bool>("--gray", () => false, "启用 Gray 编码");
        var invertOpt = new Option<bool>("--invert", () => false, "极性反转");
        var precodeOpt = new Option<bool>("--precode", () => false, "预编码");

        var cmd = new Command("config", "配置 PG 参数")
        {
            deviceIdArg,
            laneIndexArg,
            patternOpt,
            modeOpt,
            customOpt,
            swingOpt,
            grayOpt,
            invertOpt,
            precodeOpt
        };

        cmd.SetHandler(async (InvocationContext ctx) =>
        {
            var deviceId = ctx.ParseResult.GetValueForArgument(deviceIdArg);
            var laneIndex = ctx.ParseResult.GetValueForArgument(laneIndexArg);
            var pattern = ctx.ParseResult.GetValueForOption(patternOpt) ?? "PRBS31";
            var mode = ctx.ParseResult.GetValueForOption(modeOpt) ?? "SingleStream";
            var custom = ctx.ParseResult.GetValueForOption(customOpt);
            var swing = ctx.ParseResult.GetValueForOption(swingOpt);
            var gray = ctx.ParseResult.GetValueForOption(grayOpt);
            var invert = ctx.ParseResult.GetValueForOption(invertOpt);
            var precode = ctx.ParseResult.GetValueForOption(precodeOpt);

            using var scope = services.CreateScope();
            var appService = scope.ServiceProvider.GetRequiredService<IPatternGeneratorAppService>();

            var config = new PgConfigurationDto(
                Pattern: pattern,
                Mode: mode,
                CustomPattern: custom,
                MsbPattern: null,
                LsbPattern: null,
                FirTaps: null,
                SwingMillivolts: swing,
                GrayEncoding: gray,
                PolarityInvert: invert,
                PreCoding: precode
            );

            await appService.ConfigurePgAsync(deviceId, laneIndex, config);
            AnsiConsole.MarkupLine($"[green]✓ PG 配置成功: 通道 {laneIndex}, 码型 {pattern}[/]");
        });

        return cmd;
    }

    private static Command CreateEnableCommand(IServiceProvider services)
    {
        var deviceIdArg = new Argument<Guid>("deviceId", "设备 ID");
        var laneIndexArg = new Argument<int>("laneIndex", "通道索引 (0-7)");

        var cmd = new Command("enable", "启用 PG 输出")
        {
            deviceIdArg,
            laneIndexArg
        };

        cmd.SetHandler(async (Guid deviceId, int laneIndex) =>
        {
            using var scope = services.CreateScope();
            var appService = scope.ServiceProvider.GetRequiredService<IPatternGeneratorAppService>();

            await appService.EnablePgAsync(deviceId, laneIndex);
            AnsiConsole.MarkupLine($"[green]✓ PG 已启用: 通道 {laneIndex}[/]");
        }, deviceIdArg, laneIndexArg);

        return cmd;
    }

    private static Command CreateDisableCommand(IServiceProvider services)
    {
        var deviceIdArg = new Argument<Guid>("deviceId", "设备 ID");
        var laneIndexArg = new Argument<int>("laneIndex", "通道索引 (0-7)");

        var cmd = new Command("disable", "禁用 PG 输出")
        {
            deviceIdArg,
            laneIndexArg
        };

        cmd.SetHandler(async (Guid deviceId, int laneIndex) =>
        {
            using var scope = services.CreateScope();
            var appService = scope.ServiceProvider.GetRequiredService<IPatternGeneratorAppService>();

            await appService.DisablePgAsync(deviceId, laneIndex);
            AnsiConsole.MarkupLine($"[yellow]PG 已禁用: 通道 {laneIndex}[/]");
        }, deviceIdArg, laneIndexArg);

        return cmd;
    }

    private static Command CreateStatusCommand(IServiceProvider services)
    {
        var deviceIdArg = new Argument<Guid>("deviceId", "设备 ID");
        var laneIndexArg = new Argument<int?>("laneIndex", () => null, "通道索引 (可选，留空显示所有)");

        var cmd = new Command("status", "查看 PG 状态")
        {
            deviceIdArg,
            laneIndexArg
        };

        cmd.SetHandler(async (Guid deviceId, int? laneIndex) =>
        {
            using var scope = services.CreateScope();
            var deviceService = scope.ServiceProvider.GetRequiredService<IDeviceAppService>();
            var pgService = scope.ServiceProvider.GetRequiredService<IPatternGeneratorAppService>();

            var lanes = await deviceService.GetLanesAsync(deviceId);

            if (laneIndex.HasValue)
            {
                var lane = lanes.FirstOrDefault(l => l.LaneIndex == laneIndex.Value);
                if (lane == null)
                {
                    AnsiConsole.MarkupLine($"[red]通道 {laneIndex} 不存在[/]");
                    return;
                }
                var state = lane.PgEnabled ? "[green]ON[/]" : "[grey]OFF[/]";
                AnsiConsole.MarkupLine($"通道 {laneIndex}: PG={state}, 码型={lane.CurrentPattern ?? "-"}");
            }
            else
            {
                var table = new Table()
                    .Border(TableBorder.Rounded)
                    .AddColumn("[bold]通道[/]")
                    .AddColumn("[bold]PG 状态[/]")
                    .AddColumn("[bold]码型[/]");

                foreach (var lane in lanes)
                {
                    var pgState = lane.PgEnabled ? "[green]ON[/]" : "[grey]OFF[/]";
                    table.AddRow(lane.LaneIndex.ToString(), pgState, lane.CurrentPattern ?? "-");
                }

                AnsiConsole.Write(table);
            }
        }, deviceIdArg, laneIndexArg);

        return cmd;
    }
}
