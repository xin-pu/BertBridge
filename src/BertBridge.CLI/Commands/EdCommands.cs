using System.CommandLine;
using BertBridge.Application.Contracts;
using BertBridge.Application.Dtos;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;

namespace BertBridge.CLI.Commands;

/// <summary>
/// ED（误码检测器）CLI 命令。
/// </summary>
internal static class EdCommands
{
    public static Command Create(IServiceProvider services)
    {
        var edCommand = new Command("ed", "误码检测器 (ED) 管理命令");

        edCommand.AddCommand(CreateStartCommand(services));
        edCommand.AddCommand(CreateStopCommand(services));
        edCommand.AddCommand(CreateReadCommand(services));
        edCommand.AddCommand(CreateMonitorCommand(services));

        return edCommand;
    }

    private static Command CreateStartCommand(IServiceProvider services)
    {
        var deviceIdArg = new Argument<Guid>("deviceId", "设备 ID");
        var laneIndexArg = new Argument<int>("laneIndex", "通道索引 (0-7)");
        var patternOpt = new Option<string>("--pattern", () => "PRBS31", "期望码型");

        var cmd = new Command("start", "启动误码检测")
        {
            deviceIdArg,
            laneIndexArg,
            patternOpt
        };

        cmd.SetHandler(async (Guid deviceId, int laneIndex, string pattern) =>
        {
            using var scope = services.CreateScope();
            var appService = scope.ServiceProvider.GetRequiredService<IErrorDetectorAppService>();

            var result = await appService.StartEdAsync(deviceId, laneIndex, pattern);
            AnsiConsole.MarkupLine($"[green]✓ ED 已启动: 通道 {laneIndex}[/]");
            PrintEdResult(result);
        }, deviceIdArg, laneIndexArg, patternOpt);

        return cmd;
    }

    private static Command CreateStopCommand(IServiceProvider services)
    {
        var deviceIdArg = new Argument<Guid>("deviceId", "设备 ID");
        var laneIndexArg = new Argument<int>("laneIndex", "通道索引 (0-7)");

        var cmd = new Command("stop", "停止误码检测")
        {
            deviceIdArg,
            laneIndexArg
        };

        cmd.SetHandler(async (Guid deviceId, int laneIndex) =>
        {
            using var scope = services.CreateScope();
            var appService = scope.ServiceProvider.GetRequiredService<IErrorDetectorAppService>();

            var result = await appService.StopEdAsync(deviceId, laneIndex);
            AnsiConsole.MarkupLine($"[yellow]ED 已停止: 通道 {laneIndex}[/]");
            PrintEdResult(result);
        }, deviceIdArg, laneIndexArg);

        return cmd;
    }

    private static Command CreateReadCommand(IServiceProvider services)
    {
        var deviceIdArg = new Argument<Guid>("deviceId", "设备 ID");
        var laneIndexArg = new Argument<int>("laneIndex", "通道索引 (0-7)");

        var cmd = new Command("read", "读取当前 ED 结果")
        {
            deviceIdArg,
            laneIndexArg
        };

        cmd.SetHandler(async (Guid deviceId, int laneIndex) =>
        {
            using var scope = services.CreateScope();
            var appService = scope.ServiceProvider.GetRequiredService<IErrorDetectorAppService>();

            var result = await appService.ReadEdResultAsync(deviceId, laneIndex);
            PrintEdResult(result);
        }, deviceIdArg, laneIndexArg);

        return cmd;
    }

    private static Command CreateMonitorCommand(IServiceProvider services)
    {
        var deviceIdArg = new Argument<Guid>("deviceId", "设备 ID");
        var laneIndexArg = new Argument<int>("laneIndex", "通道索引 (0-7)");
        var intervalOpt = new Option<int>("--interval", () => 1, "刷新间隔 (秒)");
        var countOpt = new Option<int?>("--count", () => null, "刷新次数 (留空=持续)");

        var cmd = new Command("monitor", "实时监控 ED 数据")
        {
            deviceIdArg,
            laneIndexArg,
            intervalOpt,
            countOpt
        };

        cmd.SetHandler(async (Guid deviceId, int laneIndex, int interval, int? count) =>
        {
            using var scope = services.CreateScope();
            var appService = scope.ServiceProvider.GetRequiredService<IErrorDetectorAppService>();

            var iterations = 0;
            while (count == null || iterations < count.Value)
            {
                if (iterations > 0)
                {
                    // 清除上一行内容
                    Console.SetCursorPosition(0, Console.CursorTop - 4);
                }

                var result = await appService.ReadEdResultAsync(deviceId, laneIndex);
                PrintEdResultLive(result, iterations);
                iterations++;

                await Task.Delay(TimeSpan.FromSeconds(interval));
            }
        }, deviceIdArg, laneIndexArg, intervalOpt, countOpt);

        return cmd;
    }

    private static void PrintEdResult(EdResultDto result)
    {
        var berColor = result.Ber > 1e-6 ? "red" : result.Ber > 1e-9 ? "yellow" : "green";
        var lockIcon = result.CdrLocked && result.PllLocked && result.DspReady ? "🟢" : "🔴";

        var table = new Table()
            .Border(TableBorder.Rounded)
            .HideHeaders()
            .AddColumn("Key")
            .AddColumn("Value")
            .AddRow("误码数", $"{result.ErrorCount:N0}")
            .AddRow("总比特数", $"{result.TotalCount:N0}")
            .AddRow("BER", $"[{berColor}]{result.Ber:E2}[/]")
            .AddRow("SNR", result.SnrDb.HasValue ? $"{result.SnrDb:F2} dB" : "-")
            .AddRow("信号检测", result.SignalDetected ? "✓" : "✗")
            .AddRow("CDR 锁定", result.CdrLocked ? "✓" : "✗")
            .AddRow("PLL 锁定", result.PllLocked ? "✓" : "✗")
            .AddRow("DSP 就绪", result.DspReady ? "✓" : "✗")
            .AddRow("时间", result.Timestamp.ToString("HH:mm:ss"));

        AnsiConsole.Write(table);
    }

    private static void PrintEdResultLive(EdResultDto result, int iteration)
    {
        var berColor = result.Ber > 1e-6 ? "red" : result.Ber > 1e-9 ? "yellow" : "green";
        var lockState = result.CdrLocked && result.PllLocked && result.DspReady
            ? "[green]锁定[/]"
            : "[red]未锁定[/]";

        AnsiConsole.MarkupLine($"  [#{iteration + 1}] [{berColor}]BER: {result.Ber:E2}[/] | 错误: {result.ErrorCount:N0} | 总数: {result.TotalCount:N0} | SNR: {result.SnrDb:F1} dB | 链路: {lockState}");
    }
}
