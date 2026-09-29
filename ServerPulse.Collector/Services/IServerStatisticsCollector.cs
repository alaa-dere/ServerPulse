using ServerPulse.Collector.Models;

namespace ServerPulse.Collector.Services;

public interface IServerStatisticsCollector
{
    Task<ServerStatistics> CollectAsync(CancellationToken cancellationToken = default);
}