using System.Globalization;
using DynamicStructures.Benchmarking;
using DynamicStructures.Core;
using DynamicStructures.DataStructures;

namespace DynamicStructures.Tests;

public class BenchmarkTests
{
    [Fact]
    public void Statistics_ComputesAverageMedianMinMax()
    {
        var summary = BenchmarkStatistics.Summarize([1, 2, 3, 4]);

        Assert.Equal(2.5, summary.Average, precision: 12);
        Assert.Equal(2.5, summary.Median, precision: 12);
        Assert.Equal(1, summary.Min);
        Assert.Equal(4, summary.Max);

        var single = BenchmarkStatistics.Summarize([5]);
        Assert.Equal(5, single.Average);
        Assert.Equal(5, single.Median);

        Assert.Throws<ArgumentException>(() => BenchmarkStatistics.Summarize([]));
    }

    [Fact]
    public void ScenarioGenerator_WritesValidScenarios()
    {
        var directory = TestHelpers.TempDirectory();
        try
        {
            var scenarios = ScenarioGenerator.WriteAll(directory);

            Assert.Equal(16, scenarios.Count);
            Assert.Equal(8, scenarios.Count(scenario => scenario.Family == "Stack"));
            Assert.Equal(8, scenarios.Count(scenario => scenario.Family == "Queue"));
            Assert.Equal(scenarios.Count, scenarios.Select(scenario => scenario.Name).Distinct().Count());

            foreach (var scenario in scenarios)
            {
                Assert.True(File.Exists(scenario.FilePath));
                Assert.True(scenario.OperationCount > 0);
                Assert.Contains("total=" + scenario.OperationCount, scenario.Composition);

                var file = CommandParser.Parse(File.ReadAllText(scenario.FilePath), scenario.FileName);
                Assert.Empty(file.Issues);
                Assert.Equal(scenario.OperationCount, file.Commands.Count);

                if (scenario.Family == "Stack")
                {
                    var observer = new CollectingOperationObserver();
                    CommandExecutor.ExecuteStack(new LinkedStack<string>(), file.Commands, observer);
                    Assert.All(observer.Outcomes, outcome => Assert.Equal(OperationStatus.Ok, outcome.Status));
                }
                else
                {
                    foreach (var queue in new IQueue<string>[]
                             {
                                 new ListBasedQueue<string>(),
                                 new StandardQueue<string>()
                             })
                    {
                        var observer = new CollectingOperationObserver();
                        CommandExecutor.ExecuteQueue(queue, file.Commands, observer);
                        Assert.All(observer.Outcomes, outcome => Assert.Equal(OperationStatus.Ok, outcome.Status));
                    }
                }
            }
        }
        finally
        {
            TestHelpers.DeleteDirectory(directory);
        }
    }

    [Fact]
    public void ScenarioGenerator_IsDeterministic()
    {
        var firstDirectory = TestHelpers.TempDirectory();
        var secondDirectory = TestHelpers.TempDirectory();
        try
        {
            var first = ScenarioGenerator.WriteAll(firstDirectory);
            var second = ScenarioGenerator.WriteAll(secondDirectory);

            Assert.Equal(
                first.Select(scenario => scenario.Composition),
                second.Select(scenario => scenario.Composition));

            foreach (var scenario in first)
            {
                var otherPath = Path.Combine(secondDirectory, scenario.FileName);
                Assert.Equal(
                    File.ReadAllText(scenario.FilePath),
                    File.ReadAllText(otherPath));
            }
        }
        finally
        {
            TestHelpers.DeleteDirectory(firstDirectory);
            TestHelpers.DeleteDirectory(secondDirectory);
        }
    }

    [Fact]
    public void ReportWriter_WritesCsvAndMarkdown()
    {
        var root = TestHelpers.TempDirectory();
        var reports = Path.Combine(root, "Reports");
        try
        {
            var scenario = new ScenarioDefinition(
                "queue-synthetic",
                "Queue",
                "queue_synthetic.txt",
                Path.Combine(root, "queue_synthetic.txt"),
                2,
                "push/enqueue=1, pop/dequeue=1, total=2",
                "Синтетический сценарий для проверки отчёта.");
            File.WriteAllText(scenario.FilePath, "1,a 2");

            var records = new[]
            {
                new BenchmarkRecord(
                    scenario.Name, "Queue", 2, scenario.Composition, "ListBasedQueue",
                    1, 2, 0.5, 2.0, 2.5, scenario.FileName),
                new BenchmarkRecord(
                    scenario.Name, "Queue", 2, scenario.Composition, "ListBasedQueue",
                    2, 2, 0.6, 2.2, 2.8, scenario.FileName),
                new BenchmarkRecord(
                    scenario.Name, "Queue", 2, scenario.Composition, "StandardQueue",
                    1, 2, 0.4, 0.02, 0.42, scenario.FileName),
                new BenchmarkRecord(
                    scenario.Name, "Queue", 2, scenario.Composition, "StandardQueue",
                    2, 2, 0.3, 0.01, 0.31, scenario.FileName)
            };

            var environment = BenchmarkEnvironment.Capture(2, 1);
            var paths = BenchmarkReportWriter.WriteAll(
                records,
                [scenario],
                environment,
                reports,
                root);

            Assert.True(File.Exists(paths.AllCsv));
            Assert.True(File.Exists(paths.StackCsv));
            Assert.True(File.Exists(paths.QueueCsv));
            Assert.True(File.Exists(paths.ReportMarkdown));
            Assert.True(File.Exists(paths.RootReportMarkdown));
            Assert.True(File.Exists(paths.EnvironmentMarkdown));

            var csv = File.ReadAllText(paths.AllCsv);
            Assert.StartsWith(
                "Scenario,OperationCount,Composition,Implementation,Run,ReadMs,ExecutionMs,TotalMs,Repeats,Family,InputFile",
                csv);
            Assert.Contains("\"push/enqueue=1, pop/dequeue=1, total=2\"", csv);
            Assert.Contains("ListBasedQueue", csv);
            Assert.Contains("StandardQueue", csv);
            Assert.Equal(5, csv.Split('\n', StringSplitOptions.RemoveEmptyEntries).Length);

            var stackCsv = File.ReadAllText(paths.StackCsv);
            Assert.Single(stackCsv.Split('\n', StringSplitOptions.RemoveEmptyEntries));

            var report = File.ReadAllText(paths.ReportMarkdown);
            Assert.Contains("queue-synthetic", report);
            Assert.Contains("фактические", report);
            Assert.Contains("ListBasedQueue", report);
            Assert.Equal(report, File.ReadAllText(paths.RootReportMarkdown));

            var environmentText = File.ReadAllText(paths.EnvironmentMarkdown);
            Assert.Contains("Условия эксперимента", environmentText);
            Assert.Contains(environment.Configuration, environmentText);
            Assert.Contains(
                environment.Repeats.ToString(CultureInfo.InvariantCulture),
                environmentText);
        }
        finally
        {
            TestHelpers.DeleteDirectory(root);
        }
    }

    [Fact]
    public void ReportWriter_HandlesQueueOnlyWithMissingStackRows()
    {
        var root = TestHelpers.TempDirectory();
        var reports = Path.Combine(root, "Reports");
        try
        {
            var scenarios = new[]
            {
                new ScenarioDefinition("s", "Stack", "s.txt", Path.Combine(root, "s.txt"), 1, "total=1", "stack"),
                new ScenarioDefinition("q", "Queue", "q.txt", Path.Combine(root, "q.txt"), 1, "total=1", "queue")
            };
            var records = new[]
            {
                new BenchmarkRecord("q", "Queue", 1, "total=1", "ListBasedQueue", 1, 1, 0.1, 0.2, 0.3, "q.txt"),
                new BenchmarkRecord("q", "Queue", 1, "total=1", "StandardQueue", 1, 1, 0.1, 0.1, 0.2, "q.txt")
            };

            var paths = BenchmarkReportWriter.WriteAll(
                records,
                scenarios,
                BenchmarkEnvironment.Capture(1, 1),
                reports,
                root);

            var stackCsv = File.ReadAllText(paths.StackCsv);
            Assert.Single(stackCsv.Split('\n', StringSplitOptions.RemoveEmptyEntries));
            Assert.Contains("0.20", File.ReadAllText(paths.QueueCsv));
        }
        finally
        {
            TestHelpers.DeleteDirectory(root);
        }
    }
}
