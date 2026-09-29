using Microsoft.Extensions.Options;
using ServerPulse.Collector.Configurations;

namespace ServerPulse.Collector;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly ServerStatisticsConfig _config;

    public Worker(ILogger<Worker> logger, IOptions<ServerStatisticsConfig> options)
    {
        _logger = logger;
        _config = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Collecting statistics for server {ServerIdentifier} at {Time}", _config.ServerIdentifier, DateTimeOffset.Now);

            await Task.Delay(TimeSpan.FromSeconds(_config.SamplingIntervalSeconds), stoppingToken);
        }
    }
}