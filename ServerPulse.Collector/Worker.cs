using Microsoft.Extensions.Options;
using ServerPulse.Collector.Configurations;
using ServerPulse.Collector.Services;
using ServerPulse.Collector.Services.Messaging;

namespace ServerPulse.Collector;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly ServerStatisticsConfig _config;
    private readonly IServerStatisticsCollector _statisticsCollector;
    private readonly IMessagePublisher _messagePublisher;

    public Worker(
        ILogger<Worker> logger,
        IOptions<ServerStatisticsConfig> options,
        IServerStatisticsCollector statisticsCollector,
        IMessagePublisher messagePublisher)
    {
        _logger = logger;
        _config = options.Value;
        _statisticsCollector = statisticsCollector;
        _messagePublisher = messagePublisher;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var statistics = await _statisticsCollector.CollectAsync(stoppingToken);

            string routingKey = $"ServerStatistics.{_config.ServerIdentifier}";

            _logger.LogInformation(
                "Server: {ServerIdentifier} | CPU: {CpuUsage:F2}% | Memory Used: {MemoryUsage:F2} MB | Available Memory: {AvailableMemory:F2} MB | Time: {Timestamp}",
                _config.ServerIdentifier,
                statistics.CpuUsage,
                statistics.MemoryUsage,
                statistics.AvailableMemory,
                statistics.Timestamp);

            await _messagePublisher.PublishAsync(routingKey, statistics, stoppingToken);
            await Task.Delay(TimeSpan.FromSeconds(_config.SamplingIntervalSeconds), stoppingToken);
        }
    }
}