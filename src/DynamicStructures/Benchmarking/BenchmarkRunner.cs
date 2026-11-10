using System.Diagnostics;
using DynamicStructures.Core;
using DynamicStructures.DataStructures;
using DynamicStructures.Infrastructure;

namespace DynamicStructures.Benchmarking;

/// <summary>
/// Проводит замеры на одинаковых файлах сценариев. Печать в консоль не входит
/// в измеряемый участок: при выполнении observer равен null.
/// </summary>
public static class BenchmarkRunner
{
    public static BenchmarkRunResult Run(
        int repeats = 5,
        int warmupRuns = 1,
        TextWriter? progress = null,
        string? scenariosDirectory = null,
        string? reportsDirectory = null,
        string? rootDirectory = null)
    {
        if (repeats <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(repeats), "Количество повторов должно быть положительным.");
        }

        if (warmupRuns < 1)
        {
            warmupRuns = 1;
        }

        var root = rootDirectory ?? ProjectPaths.RepositoryRoot;
        var scenarioDirectory = scenariosDirectory ?? Path.Combine(root, "data", "scenarios");
        var reportDirectory = reportsDirectory ?? Path.Combine(root, "Reports");

        Directory.CreateDirectory(scenarioDirectory);
        Directory.CreateDirectory(reportDirectory);

        var environment = BenchmarkEnvironment.Capture(repeats, warmupRuns);
        var scenarios = ScenarioGenerator.WriteAll(scenarioDirectory);
        var records = new List<BenchmarkRecord>();

        foreach (var scenario in scenarios)
        {
            progress?.WriteLine($"Сценарий {scenario.Name}: {scenario.OperationCount} операций...");
            var runners = CreateRunners(scenario.Family);

            var validation = CommandParser.Load(scenario.FilePath);
            if (validation.Issues.Count > 0)
            {
                throw new InvalidOperationException(
                    $"Сценарий {scenario.Name} содержит ошибки разбора: {validation.Issues[0].Message}");
            }

            foreach (var runner in runners)
            {
                for (var warmup = 0; warmup < warmupRuns; warmup++)
                {
                    runner.Reset();
                    var observer = new CollectingOperationObserver();
                    runner.Execute(validation.Commands, observer);
                    if (warmup == 0)
                    {
                        ValidateOutcomes(scenario, runner.Name, observer.Outcomes);
                    }
                }
            }

            for (var run = 1; run <= repeats; run++)
            {
                foreach (var runner in runners)
                {
                    runner.Reset();
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    GC.Collect();

                    var totalWatch = Stopwatch.StartNew();
                    var readWatch = Stopwatch.StartNew();
                    var file = CommandParser.Load(scenario.FilePath);
                    readWatch.Stop();

                    if (file.Issues.Count > 0)
                    {
                        throw new InvalidOperationException(
                            $"Сценарий {scenario.Name} содержит ошибки разбора: {file.Issues[0].Message}");
                    }

                    var executionWatch = Stopwatch.StartNew();
                    runner.Execute(file.Commands, observer: null);
                    executionWatch.Stop();
                    totalWatch.Stop();

                    records.Add(new BenchmarkRecord(
                        scenario.Name,
                        scenario.Family,
                        scenario.OperationCount,
                        scenario.Composition,
                        runner.Name,
                        run,
                        repeats,
                        readWatch.Elapsed.TotalMilliseconds,
                        executionWatch.Elapsed.TotalMilliseconds,
                        totalWatch.Elapsed.TotalMilliseconds,
                        scenario.FileName));
                }
            }

            progress?.WriteLine($"  готово: {repeats} повторов для {runners.Count} реализаций.");
        }

        var paths = BenchmarkReportWriter.WriteAll(
            records,
            scenarios,
            environment,
            reportDirectory,
            root);

        return new BenchmarkRunResult(records, scenarios, environment, reportDirectory, paths.RootReportMarkdown);
    }

    private static List<TargetRunner> CreateRunners(string family)
    {
        if (family == "Stack")
        {
            return [new StackRunner()];
        }

        if (family == "Queue")
        {
            return
            [
                new QueueRunner("ListBasedQueue", () => new ListBasedQueue<string>()),
                new QueueRunner("StandardQueue", () => new StandardQueue<string>())
            ];
        }

        throw new ArgumentException($"Неизвестное семейство сценариев: {family}.");
    }

    private static void ValidateOutcomes(
        ScenarioDefinition scenario,
        string implementation,
        IReadOnlyList<OperationOutcome> outcomes)
    {
        foreach (var outcome in outcomes)
        {
            if (outcome.Status != OperationStatus.Ok)
            {
                throw new InvalidOperationException(
                    $"Сценарий {scenario.Name} ({implementation}) содержит недопустимую операцию " +
                    $"[{outcome.Number}] '{outcome.Raw}': {outcome.Status} {outcome.Error}");
            }
        }
    }

    private abstract class TargetRunner
    {
        public abstract string Name { get; }

        public abstract void Reset();

        public abstract void Execute(IReadOnlyList<Command> commands, IOperationObserver? observer);
    }

    private sealed class StackRunner : TargetRunner
    {
        private LinkedStack<string> _stack = new();

        public override string Name => "LinkedStack";

        public override void Reset() => _stack = new LinkedStack<string>();

        public override void Execute(IReadOnlyList<Command> commands, IOperationObserver? observer) =>
            CommandExecutor.ExecuteStack(_stack, commands, observer);
    }

    private sealed class QueueRunner : TargetRunner
    {
        private readonly Func<IQueue<string>> _factory;
        private IQueue<string> _queue;

        public QueueRunner(string name, Func<IQueue<string>> factory)
        {
            Name = name;
            _factory = factory;
            _queue = factory();
        }

        public override string Name { get; }

        public override void Reset() => _queue = _factory();

        public override void Execute(IReadOnlyList<Command> commands, IOperationObserver? observer) =>
            CommandExecutor.ExecuteQueue(_queue, commands, observer);
    }
}
