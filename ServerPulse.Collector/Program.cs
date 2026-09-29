using ServerPulse.Collector;
using ServerPulse.Collector.Configurations;
using ServerPulse.Collector.Services;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<ServerStatisticsConfig>(
    builder.Configuration.GetSection("ServerStatisticsConfig"));

builder.Services.AddSingleton<IServerStatisticsCollector, ServerStatisticsCollector>();

builder.Services.AddHostedService<Worker>();

var host = builder.Build();

host.Run();