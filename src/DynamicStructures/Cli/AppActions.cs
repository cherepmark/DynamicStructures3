using System.Globalization;
using DynamicStructures.Applications;
using DynamicStructures.Benchmarking;
using DynamicStructures.Core;
using DynamicStructures.DataStructures;
using DynamicStructures.Demo;
using DynamicStructures.Expressions;
using DynamicStructures.Infrastructure;
using DynamicStructures.Trees;

namespace DynamicStructures.Cli;

public static class AppActions
{
    public static void RunStackFile(string? path, TextWriter writer)
    {
        ArgumentNullException.ThrowIfNull(writer);
        var resolved = ProjectPaths.ResolveExistingFile(string.IsNullOrWhiteSpace(path) ? "input.txt" : path, "input.txt");
        var file = CommandParser.Load(resolved);

        writer.WriteLine($"Стек: файл {file.Source}");
        writer.WriteLine($"Команд: {file.Commands.Count}; ошибок разбора: {file.Issues.Count}");
        CommandExecutor.WriteParseIssues(file, writer);
        if (file.Issues.Count > 0)
        {
            writer.WriteLine("Корректные команды будут выполнены, ошибочные токены пропущены.");
        }

        var stack = new LinkedStack<string>();
        var observer = new ConsoleOperationObserver(writer, "стек", isStack: true);
        CommandExecutor.ExecuteStack(stack, file.Commands, observer);
        writer.WriteLine($"Итог: размер = {stack.Count}; пусто = {stack.IsEmpty}.");
    }

    public static void RunQueueFile(string? path, bool standard, TextWriter writer)
    {
        ArgumentNullException.ThrowIfNull(writer);
        var resolved = ProjectPaths.ResolveExistingFile(string.IsNullOrWhiteSpace(path) ? "input.txt" : path, "input.txt");
        var file = CommandParser.Load(resolved);
        IQueue<string> queue = standard ? new StandardQueue<string>() : new ListBasedQueue<string>();

        writer.WriteLine($"Очередь ({queue.ImplementationName}): файл {file.Source}");
        writer.WriteLine($"Команд: {file.Commands.Count}; ошибок разбора: {file.Issues.Count}");
        CommandExecutor.WriteParseIssues(file, writer);
        if (file.Issues.Count > 0)
        {
            writer.WriteLine("Корректные команды будут выполнены, ошибочные токены пропущены.");
        }

        var observer = new ConsoleOperationObserver(writer, "очередь", isStack: false);
        CommandExecutor.ExecuteQueue(queue, file.Commands, observer);
        writer.WriteLine($"Итог: размер = {queue.Count}; пусто = {queue.IsEmpty}.");
    }

    public static void EvaluatePostfixExpression(string expression, TextWriter writer)
    {
        ArgumentNullException.ThrowIfNull(writer);
        var result = PostfixCalculator.Evaluate(expression);
        writer.WriteLine($"Постфиксная запись: {result.NormalizedPostfix}");
        if (result.Success)
        {
            writer.WriteLine($"Результат: {PostfixCalculator.FormatValue(result.Value)}");
        }
        else
        {
            writer.WriteLine($"Ошибка: {result.Error}");
        }
    }

    public static void EvaluatePostfixFile(string? path, TextWriter writer)
    {
        ArgumentNullException.ThrowIfNull(writer);
        var resolved = ProjectPaths.ResolveExistingFile(string.IsNullOrWhiteSpace(path) ? "postfix.txt" : path, "data/postfix.txt");
        var lines = ExpressionFile.Load(resolved);
        writer.WriteLine($"Постфиксные выражения: {resolved}; строк: {lines.Count}");
        foreach (var line in lines)
        {
            writer.WriteLine($"[{line.LineNumber}] {line.Expression}");
            var result = PostfixCalculator.Evaluate(line.Expression);
            if (result.Success)
            {
                writer.WriteLine(
                    $"    RPN: {result.NormalizedPostfix}; значение = {PostfixCalculator.FormatValue(result.Value)}");
            }
            else
            {
                writer.WriteLine($"    Ошибка: {result.Error}");
            }
        }
    }

    public static void ConvertInfixExpression(string expression, TextWriter writer)
    {
        ArgumentNullException.ThrowIfNull(writer);
        var conversion = ExpressionConverter.Convert(expression);
        if (!conversion.Success || conversion.Postfix is null)
        {
            writer.WriteLine($"Ошибка преобразования: {conversion.Error}");
            return;
        }

        writer.WriteLine($"Инфикс: {expression}");
        writer.WriteLine($"Постфикс: {conversion.Postfix}");
        var evaluation = PostfixCalculator.Evaluate(conversion.Postfix);
        if (evaluation.Success)
        {
            writer.WriteLine($"Значение: {PostfixCalculator.FormatValue(evaluation.Value)}");
        }
        else
        {
            writer.WriteLine($"Ошибка вычисления: {evaluation.Error}");
        }
    }

    public static void ConvertInfixFile(string? path, TextWriter writer)
    {
        ArgumentNullException.ThrowIfNull(writer);
        var resolved = ProjectPaths.ResolveExistingFile(string.IsNullOrWhiteSpace(path) ? "infix.txt" : path, "data/infix.txt");
        var lines = ExpressionFile.Load(resolved);
        writer.WriteLine($"Инфиксные выражения: {resolved}; строк: {lines.Count}");
        foreach (var line in lines)
        {
            writer.WriteLine($"[{line.LineNumber}] {line.Expression}");
            ConvertInfixExpression(line.Expression, writer);
        }
    }

    public static void RunTreeBuilder(string? prefix, TextWriter writer)
    {
        ArgumentNullException.ThrowIfNull(writer);
        var input = string.IsNullOrWhiteSpace(prefix) ? TreeBuilder.DefaultPrefix : prefix;

        writer.WriteLine($"Префиксная запись: {input}");
        if (!TreeBuilder.TryBuild(input, out var root, out var error))
        {
            writer.WriteLine($"Ошибка: {error}");
            return;
        }

        writer.WriteLine(
            $"Узлов: {TreeBuilder.CountNodes(root)}; листьев: {TreeBuilder.CountLeaves(root)}; " +
            $"высота: {TreeBuilder.Height(root)}.");
        writer.WriteLine($"Preorder: {TreeBuilder.Preorder(root)}");
        writer.WriteLine($"Inorder: {TreeBuilder.Inorder(root)}");
        writer.WriteLine($"Postorder: {TreeBuilder.Postorder(root)}");
        writer.WriteLine("Дерево (L/R, * — пустая связь):");
        writer.WriteLine(TreeBuilder.ToAscii(root));
        writer.WriteLine($"Обратная префиксная запись: {TreeBuilder.SerializePrefix(root)}");
    }

    public static void MeasureStackFile(string? path, int repeats, TextWriter writer)
    {
        ArgumentNullException.ThrowIfNull(writer);
        if (repeats <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(repeats));
        }

        var resolved = ProjectPaths.ResolveExistingFile(string.IsNullOrWhiteSpace(path) ? "input.txt" : path, "input.txt");
        var reads = new List<double>();
        var executions = new List<double>();
        var totals = new List<double>();

        writer.WriteLine($"Замер стека: {resolved}; повторов: {repeats}; вывод не входит в измерение.");
        for (var run = 1; run <= repeats; run++)
        {
            var totalWatch = System.Diagnostics.Stopwatch.StartNew();
            var readWatch = System.Diagnostics.Stopwatch.StartNew();
            var file = CommandParser.Load(resolved);
            readWatch.Stop();

            var stack = new LinkedStack<string>();
            var executionWatch = System.Diagnostics.Stopwatch.StartNew();
            CommandExecutor.ExecuteStack(stack, file.Commands, observer: null);
            executionWatch.Stop();
            totalWatch.Stop();

            var readMs = readWatch.Elapsed.TotalMilliseconds;
            var executionMs = executionWatch.Elapsed.TotalMilliseconds;
            var totalMs = totalWatch.Elapsed.TotalMilliseconds;
            reads.Add(readMs);
            executions.Add(executionMs);
            totals.Add(totalMs);
            writer.WriteLine(
                $"  запуск {run}: чтение+разбор {readMs:F3} мс, операции {executionMs:F3} мс, всего {totalMs:F3} мс");
        }

        writer.WriteLine(
            $"Среднее: чтение+разбор {reads.Average():F3} мс, операции {executions.Average():F3} мс, всего {totals.Average():F3} мс.");
    }

    public static void RunListDemo(TextWriter writer)
    {
        ArgumentNullException.ThrowIfNull(writer);
        LinkedListDemo.RunAll(writer);
    }

    public static void RunApplications(TextWriter writer)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ApplicationDemos.RunAll(writer);
    }

    public static BenchmarkRunResult RunBenchmarks(int repeats, TextWriter progress)
    {
        ArgumentNullException.ThrowIfNull(progress);
        var result = BenchmarkRunner.Run(repeats, warmupRuns: 1, progress);
        progress.WriteLine();
        progress.WriteLine("Отчёты записаны:");
        progress.WriteLine($"  CSV (все): {Path.Combine(result.ReportDirectory, "benchmark_all.csv")}");
        progress.WriteLine($"  CSV (стек): {Path.Combine(result.ReportDirectory, "benchmark_stack.csv")}");
        progress.WriteLine($"  CSV (очереди): {Path.Combine(result.ReportDirectory, "benchmark_queue.csv")}");
        progress.WriteLine($"  Markdown: {Path.Combine(result.ReportDirectory, "Report.md")}");
        progress.WriteLine($"  Корневой Report.md: {result.RootReportPath}");
        return result;
    }

    public static void GenerateScenarios(TextWriter writer)
    {
        ArgumentNullException.ThrowIfNull(writer);
        var directory = ProjectPaths.ScenariosDirectory;
        var scenarios = ScenarioGenerator.WriteAll(directory);
        writer.WriteLine($"Сценарии записаны в {directory}:");
        foreach (var scenario in scenarios)
        {
            writer.WriteLine(
                $"  {scenario.Name}: {scenario.OperationCount} операций, {scenario.FileName}");
        }
    }

    public static void PrintPaths(TextWriter writer)
    {
        ArgumentNullException.ThrowIfNull(writer);
        writer.WriteLine($"Корень проекта: {ProjectPaths.RepositoryRoot}");
        writer.WriteLine($"input.txt: {ProjectPaths.InputFile}");
        writer.WriteLine($"Данные: {ProjectPaths.DataDirectory}");
        writer.WriteLine($"Сценарии: {ProjectPaths.ScenariosDirectory}");
        writer.WriteLine($"Отчёты: {ProjectPaths.ReportsDirectory}");
    }

    public static bool Verify(TextWriter writer)
    {
        ArgumentNullException.ThrowIfNull(writer);
        var checks = new List<(string Name, bool Ok, string Detail)>();

        void Check(string name, bool ok, string detail = "")
        {
            checks.Add((name, ok, detail));
        }

        try
        {
            var input = CommandParser.Load(ProjectPaths.ResolveExistingFile("input.txt"));
            Check("input.txt разобран без ошибок", input.Issues.Count == 0 && input.Commands.Count > 0,
                $"команд: {input.Commands.Count}, ошибок: {input.Issues.Count}");

            var stackObserver = new CollectingOperationObserver();
            CommandExecutor.ExecuteStack(new LinkedStack<string>(), input.Commands, stackObserver);
            Check("стек выполнил input.txt без Invalid",
                stackObserver.Outcomes.All(outcome => outcome.Status != OperationStatus.Invalid),
                $"операций: {stackObserver.Outcomes.Count}");

            var listQueueObserver = new CollectingOperationObserver();
            CommandExecutor.ExecuteQueue(new ListBasedQueue<string>(), input.Commands, listQueueObserver);
            Check("ListBasedQueue выполнила input.txt без Invalid",
                listQueueObserver.Outcomes.All(outcome => outcome.Status != OperationStatus.Invalid));

            var standardQueueObserver = new CollectingOperationObserver();
            CommandExecutor.ExecuteQueue(new StandardQueue<string>(), input.Commands, standardQueueObserver);
            Check("StandardQueue выполнила input.txt без Invalid",
                standardQueueObserver.Outcomes.All(outcome => outcome.Status != OperationStatus.Invalid));

            var postfix = PostfixCalculator.Evaluate("78+32+/");
            Check("78+32+/ = 3", postfix.Success && Math.Abs(postfix.Value - 3) < 1e-12,
                postfix.Success ? postfix.Value.ToString(CultureInfo.InvariantCulture) : postfix.Error ?? "");

            var postfixSpaced = PostfixCalculator.Evaluate("2 3 + 4 *");
            Check("2 3 + 4 * = 20", postfixSpaced.Success && Math.Abs(postfixSpaced.Value - 20) < 1e-12,
                postfixSpaced.Success ? postfixSpaced.Value.ToString(CultureInfo.InvariantCulture) : postfixSpaced.Error ?? "");

            var invalidPostfix = PostfixCalculator.Evaluate("2 +");
            Check("2 + отклонено", !invalidPostfix.Success, invalidPostfix.Error ?? "");

            var conversion = ExpressionConverter.Convert("sin(0)+sqrt(9)");
            var convertedValue = conversion.Success && conversion.Postfix is not null
                ? PostfixCalculator.Evaluate(conversion.Postfix)
                : null;
            Check("sin(0)+sqrt(9) -> RPN -> 3",
                conversion.Success && convertedValue is { Success: true } && Math.Abs(convertedValue.Value - 3) < 1e-12,
                conversion.Postfix ?? conversion.Error ?? "");

            var invalidInfix = ExpressionConverter.Convert("(2+3");
            Check("(2+3 отклонено", !invalidInfix.Success, invalidInfix.Error ?? "");

            var demoOutput = new StringWriter();
            RunListDemo(demoOutput);
            Check("демонстрация 12 операций списка выполнена", demoOutput.ToString().Contains("12. Обмен"));

            var tree = TreeBuilder.Build(TreeBuilder.DefaultPrefix);
            Check("дерево из префиксной записи: 9 узлов, 4 листа, высота 4",
                TreeBuilder.CountNodes(tree) == 9 &&
                TreeBuilder.CountLeaves(tree) == 4 &&
                TreeBuilder.Height(tree) == 4,
                $"узлов: {TreeBuilder.CountNodes(tree)}, листьев: {TreeBuilder.CountLeaves(tree)}, " +
                $"высота: {TreeBuilder.Height(tree)}");
            Check("дерево: обходы и обратная префиксная запись",
                TreeBuilder.Preorder(tree) == "A B D G C E F H J" &&
                TreeBuilder.Inorder(tree) == "D G B A E C H F J" &&
                TreeBuilder.Postorder(tree) == "G D B E H J F C A" &&
                TreeBuilder.SerializePrefix(tree) == TreeBuilder.DefaultPrefix,
                TreeBuilder.Preorder(tree));

            var applicationsOutput = new StringWriter();
            RunApplications(applicationsOutput);
            Check("четыре примера приложений выполнены",
                applicationsOutput.ToString().Contains("1. Список") &&
                applicationsOutput.ToString().Contains("4. Дерево"));
        }
        catch (Exception exception)
        {
            Check("исключение при проверке", false, exception.Message);
        }

        var failed = 0;
        foreach (var check in checks)
        {
            writer.WriteLine($"{(check.Ok ? "PASS" : "FAIL")}  {check.Name}" +
                             (string.IsNullOrEmpty(check.Detail) ? string.Empty : $" — {check.Detail}"));
            if (!check.Ok)
            {
                failed++;
            }
        }

        writer.WriteLine($"Итог: {checks.Count - failed} из {checks.Count} проверок пройдено.");
        return failed == 0;
    }
}
