using System.CommandLine;
using BertBridge.Application;
using BertBridge.Infrastructure;
using BertBridge.Plugins.Mock;
using BertBridge.CLI.Commands;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration(config =>
    {
        config.SetBasePath(AppDomain.CurrentDomain.BaseDirectory);
        config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
    })
    .ConfigureServices((ctx, services) =>
    {
        // 注册 Application + Infrastructure
        services.AddBertBridgeApplication();
        services.AddBertBridgeInfrastructure(ctx.Configuration);

        // 开发阶段直接注册 Mock 插件
        if (ctx.HostingEnvironment.IsDevelopment() ||
            ctx.Configuration.GetValue<bool>("Plugins:EnableMock"))
        {
            services.AddMockPlugin();
        }
    })
    .Build();

// 确保数据库已创建
using (var scope = host.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<BertBridge.Infrastructure.Persistence.BertBridgeDbContext>();
    await dbContext.Database.EnsureCreatedAsync();
}

// ── 构建命令树 ──
var rootCommand = new RootCommand("BertBridge - 通用误码仪控制框架 CLI");

// 子命令
rootCommand.AddCommand(DeviceCommands.Create(host.Services));
rootCommand.AddCommand(PgCommands.Create(host.Services));
rootCommand.AddCommand(EdCommands.Create(host.Services));
rootCommand.AddCommand(FecCommands.Create(host.Services));
rootCommand.AddCommand(SessionCommands.Create(host.Services));

return await rootCommand.InvokeAsync(args);
