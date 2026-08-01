using System.CommandLine;
using BertBridge.Application.Contracts;
using BertBridge.Application.Dtos;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;

namespace BertBridge.CLI.Commands;

/// <summary>
/// 测试会话 CLI 命令。
/// </summary>
internal static class SessionCommands
{
    public static Command Create(IServiceProvider services)
    {
        var sessionCommand = new Command("session", "测试会话管理命令");

        sessionCommand.AddCommand(CreateCreateCommand(services));
        sessionCommand.AddCommand(CreateListCommand(services));
        sessionCommand.AddCommand(CreateInfoCommand(services));
        sessionCommand.AddCommand(CreateStartCommand(services));
        sessionCommand.AddCommand(CreateStopCommand(services));
        sessionCommand.AddCommand(CreateAbortCommand(services));

        return sessionCommand;
    }

    private static Command CreateCreateCommand(IServiceProvider services)
    {
        var deviceIdArg = new Argument<Guid>("deviceId", "设备 ID");
        var durationOpt = new Option<int?>("--duration", () => null, "测试时长 (秒)");

        var cmd = new Command("create", "创建测试会话")
        {
            deviceIdArg,
            durationOpt
        };

        cmd.SetHandler(async (Guid deviceId, int? duration) =>
        {
            using var scope = services.CreateScope();
            var appService = scope.ServiceProvider.GetRequiredService<ITestSessionAppService>();

            var dto = new CreateTestSessionDto(
                DeviceId: deviceId,
                LaneCount: 0,
                PatternsJson: "[]",
                Duration: duration.HasValue ? TimeSpan.FromSeconds(duration.Value) : null
            );

            var session = await appService.CreateSessionAsync(dto);
            AnsiConsole.MarkupLine($"[green]✓ 测试会话已创建[/]");
            AnsiConsole.MarkupLine($"  ID: {session.Id}");
            AnsiConsole.MarkupLine($"  设备: {session.DeviceId}");
            AnsiConsole.MarkupLine($"  状态: {session.Status}");
        }, deviceIdArg, durationOpt);

        return cmd;
    }

    private static Command CreateListCommand(IServiceProvider services)
    {
        var deviceIdArg = new Argument<Guid>("deviceId", "设备 ID");

        var cmd = new Command("list", "列出设备的所有测试会话")
        {
            deviceIdArg
        };

        cmd.SetHandler(async (Guid deviceId) =>
        {
            using var scope = services.CreateScope();
            var appService = scope.ServiceProvider.GetRequiredService<ITestSessionAppService>();

            var sessions = await appService.GetSessionsByDeviceAsync(deviceId);

            if (sessions.Count == 0)
            {
                AnsiConsole.MarkupLine("[yellow]没有测试会话[/]");
                return;
            }

            var table = new Table()
                .Border(TableBorder.Rounded)
                .AddColumn(new TableColumn("[bold]ID[/]").Centered())
                .AddColumn("[bold]状态[/]")
                .AddColumn("[bold]开始时间[/]")
                .AddColumn("[bold]时长[/]")
                .AddColumn("[bold]BER[/]");

            foreach (var s in sessions)
            {
                var statusColor = s.Status switch
                {
                    "Running" => "green",
                    "Completed" => "blue",
                    "Aborted" => "red",
                    _ => "grey"
                };

                table.AddRow(
                    s.Id.ToString("D")[..8] + "...",
                    $"[{statusColor}]{s.Status}[/]",
                    s.StartedAt?.ToString("HH:mm:ss") ?? "-",
                    s.Duration.ToString(@"hh\:mm\:ss"),
                    s.SummaryBer.HasValue ? $"{s.SummaryBer:E2}" : "-"
                );
            }

            AnsiConsole.Write(table);
        }, deviceIdArg);

        return cmd;
    }

    private static Command CreateInfoCommand(IServiceProvider services)
    {
        var sessionIdArg = new Argument<Guid>("sessionId", "会话 ID");

        var cmd = new Command("info", "查看会话详情")
        {
            sessionIdArg
        };

        cmd.SetHandler(async (Guid sessionId) =>
        {
            using var scope = services.CreateScope();
            var appService = scope.ServiceProvider.GetRequiredService<ITestSessionAppService>();

            var session = await appService.GetSessionAsync(sessionId);
            if (session == null)
            {
                AnsiConsole.MarkupLine("[red]会话不存在[/]");
                return;
            }

            var panel = new Panel($"""
                [bold]测试会话详情[/bold]
                ────────────────
                ID:          {session.Id}
                设备 ID:     {session.DeviceId}
                状态:        {session.Status}
                开始时间:    {session.StartedAt?.ToString("yyyy-MM-dd HH:mm:ss") ?? "-"}
                结束时间:    {session.CompletedAt?.ToString("yyyy-MM-dd HH:mm:ss") ?? "-"}
                时长:        {session.Duration:hh\:mm\:ss}
                BER 汇总:    {(session.SummaryBer.HasValue ? session.SummaryBer.Value.ToString("E2") : "-")}
                备注:        {session.Notes ?? "-"}
                数据点数:    {session.DataPointCount}
                """)
            {
                Border = BoxBorder.Rounded,
                Header = new PanelHeader(" Test Session ")
            };

            AnsiConsole.Write(panel);
        }, sessionIdArg);

        return cmd;
    }

    private static Command CreateStartCommand(IServiceProvider services)
    {
        var sessionIdArg = new Argument<Guid>("sessionId", "会话 ID");

        var cmd = new Command("start", "启动测试会话")
        {
            sessionIdArg
        };

        cmd.SetHandler(async (Guid sessionId) =>
        {
            using var scope = services.CreateScope();
            var appService = scope.ServiceProvider.GetRequiredService<ITestSessionAppService>();

            await appService.StartSessionAsync(sessionId);
            AnsiConsole.MarkupLine($"[green]✓ 测试会话已启动: {sessionId}[/]");
        }, sessionIdArg);

        return cmd;
    }

    private static Command CreateStopCommand(IServiceProvider services)
    {
        var sessionIdArg = new Argument<Guid>("sessionId", "会话 ID");

        var cmd = new Command("complete", "完成测试会话")
        {
            sessionIdArg
        };

        cmd.SetHandler(async (Guid sessionId) =>
        {
            using var scope = services.CreateScope();
            var appService = scope.ServiceProvider.GetRequiredService<ITestSessionAppService>();

            await appService.CompleteSessionAsync(sessionId);
            AnsiConsole.MarkupLine($"[green]✓ 测试会话已完成: {sessionId}[/]");
        }, sessionIdArg);

        return cmd;
    }

    private static Command CreateAbortCommand(IServiceProvider services)
    {
        var sessionIdArg = new Argument<Guid>("sessionId", "会话 ID");
        var reasonOpt = new Option<string>("--reason", () => "用户中止", "中止原因");

        var cmd = new Command("abort", "中止测试会话")
        {
            sessionIdArg,
            reasonOpt
        };

        cmd.SetHandler(async (Guid sessionId, string reason) =>
        {
            using var scope = services.CreateScope();
            var appService = scope.ServiceProvider.GetRequiredService<ITestSessionAppService>();

            await appService.AbortSessionAsync(sessionId, reason);
            AnsiConsole.MarkupLine($"[yellow]⚠ 测试会话已中止: {sessionId}[/]");
        }, sessionIdArg, reasonOpt);

        return cmd;
    }
}
