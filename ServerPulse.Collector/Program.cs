using ServerPulse.Collector;
using ServerPulse.Collector.Configurations;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<ServerStatisticsConfig>(builder.Configuration.GetSection("ServerStatisticsConfig"));

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();