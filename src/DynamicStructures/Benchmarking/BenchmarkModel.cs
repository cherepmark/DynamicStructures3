using System.Diagnostics;
using System.Runtime.InteropServices;

namespace DynamicStructures.Benchmarking;

public sealed record BenchmarkEnvironment(
    DateTimeOffset TimestampUtc,
    string OsDescription,
    string OsArchitecture,
    string ProcessArchitecture,
    string FrameworkDescription,
    string RuntimeVersion,
    int ProcessorCount,
    long StopwatchFrequency,
    long TotalAvailableMemoryBytes,
    string Configuration,
    int Repeats,
    int WarmupRuns)
{
    public static BenchmarkEnvironment Capture(int repeats, int warmupRuns)
    {
#if DEBUG
        const string configuration = "Debug";
#else
        const string configuration = "Release";
#endif

        return new BenchmarkEnvironment(
            DateTimeOffset.UtcNow,
            RuntimeInformation.OSDescription,
            RuntimeInformation.OSArchitecture.ToString(),
            RuntimeInformation.ProcessArchitecture.ToString(),
            RuntimeInformation.FrameworkDescription,
            Environment.Version.ToString(),
            Environment.ProcessorCount,
            Stopwatch.Frequency,
            GC.GetGCMemoryInfo().TotalAvailableMemoryBytes,
            configuration,
            repeats,
            warmupRuns);
    }
}

public sealed record BenchmarkRecord(
    string Scenario,
    string Family,
    int OperationCount,
    string Composition,
    string Implementation,
    int Run,
    int Repeats,
    double ReadMs,
    double ExecutionMs,
    double TotalMs,
    string InputFile);

public sealed record MetricSummary(double Average, double Median, double Min, double Max);

public static class BenchmarkStatistics
{
    public static MetricSummary Summarize(IEnumerable<double> values)
    {
        ArgumentNullException.ThrowIfNull(values);
        var data = values.ToArray();
        if (data.Length == 0)
        {
            throw new ArgumentException("Нет значений для расчёта статистики.", nameof(values));
        }

        Array.Sort(data);
        var average = data.Average();
        var median = data.Length % 2 == 1
            ? data[data.Length / 2]
            : (data[data.Length / 2 - 1] + data[data.Length / 2]) / 2.0;

        return new MetricSummary(average, median, data[0], data[^1]);
    }
}

public sealed record BenchmarkRunResult(
    IReadOnlyList<BenchmarkRecord> Records,
    IReadOnlyList<ScenarioDefinition> Scenarios,
    BenchmarkEnvironment Environment,
    string ReportDirectory,
    string RootReportPath);
