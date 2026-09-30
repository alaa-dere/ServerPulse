using ServerPulse.Collector;
using ServerPulse.Collector.Configurations;
using ServerPulse.Collector.Services;
using ServerPulse.Collector.Services.Messaging;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<ServerStatisticsConfig>(builder.Configuration.GetSection("ServerStatisticsConfig"));
builder.Services.Configure<RabbitMqConfig>(builder.Configuration.GetSection("RabbitMqConfig"));
builder.Services.AddSingleton<IServerStatisticsCollector, ServerStatisticsCollector>();
builder.Services.AddSingleton<IMessagePublisher, RabbitMqMessagePublisher>();

builder.Services.AddHostedService<Worker>();

var host = builder.Build();

host.Run();