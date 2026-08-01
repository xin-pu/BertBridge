using BertBridge.Application.Contracts;
using BertBridge.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace BertBridge.Application;

/// <summary>
/// Application 层 DI 注册扩展。
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// 注册所有 Application 层服务。
    /// </summary>
    public static IServiceCollection AddBertBridgeApplication(this IServiceCollection services)
    {
        // 应用服务
        services.AddScoped<IDeviceAppService, DeviceAppService>();
        services.AddScoped<IPatternGeneratorAppService, PatternGeneratorAppService>();
        services.AddScoped<IErrorDetectorAppService, ErrorDetectorAppService>();
        services.AddScoped<IFecAppService, FecAppService>();
        services.AddScoped<ITestSessionAppService, TestSessionAppService>();

        // MediatR (自动发现 INotificationHandler)
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);
        });

        // AutoMapper
        services.AddAutoMapper(typeof(DependencyInjection).Assembly);

        return services;
    }
}
