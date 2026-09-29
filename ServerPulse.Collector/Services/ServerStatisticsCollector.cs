using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using ServerPulse.Collector.Models;

namespace ServerPulse.Collector.Services;

[SupportedOSPlatform("windows")]
public class ServerStatisticsCollector : IServerStatisticsCollector, IDisposable
{
    private readonly PerformanceCounter _cpuCounter;

    public ServerStatisticsCollector()
    {
        _cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
    }

    public async Task<ServerStatistics> CollectAsync(CancellationToken cancellationToken = default)
    {
        _cpuCounter.NextValue();

        await Task.Delay(500, cancellationToken);

        double cpuUsage = _cpuCounter.NextValue();

        MEMORYSTATUSEX memoryStatus = new();
        memoryStatus.dwLength = (uint)Marshal.SizeOf<MEMORYSTATUSEX>();

        if (!GlobalMemoryStatusEx(ref memoryStatus))
        {
            throw new InvalidOperationException("Unable to retrieve system memory information.");
        }

        double totalMemory = memoryStatus.ullTotalPhys / 1024d / 1024d;

        double availableMemory = memoryStatus.ullAvailPhys / 1024d / 1024d;

        double memoryUsage = totalMemory - availableMemory;

        return new ServerStatistics
        {
            MemoryUsage = memoryUsage,
            AvailableMemory = availableMemory,
            CpuUsage = cpuUsage,
            Timestamp = DateTime.UtcNow
        };
    }

    public void Dispose()
    {
        _cpuCounter.Dispose();
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GlobalMemoryStatusEx(ref MEMORYSTATUSEX lpBuffer);

    [StructLayout(LayoutKind.Sequential)]
    private struct MEMORYSTATUSEX
    {
        public uint dwLength;
        public uint dwMemoryLoad;
        public ulong ullTotalPhys;
        public ulong ullAvailPhys;
        public ulong ullTotalPageFile;
        public ulong ullAvailPageFile;
        public ulong ullTotalVirtual;
        public ulong ullAvailVirtual;
        public ulong ullAvailExtendedVirtual;
    }
}