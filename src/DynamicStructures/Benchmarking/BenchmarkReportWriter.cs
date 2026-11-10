using System.Globalization;
using System.Text;

namespace DynamicStructures.Benchmarking;

public sealed record BenchmarkReportPaths(
    string AllCsv,
    string StackCsv,
    string QueueCsv,
    string EnvironmentMarkdown,
    string StackMarkdown,
    string QueueMarkdown,
    string ReportMarkdown,
    string RootReportMarkdown);

public static class BenchmarkReportWriter
{
    private static readonly CultureInfo Invariant = CultureInfo.InvariantCulture;

    public static BenchmarkReportPaths WriteAll(
        IReadOnlyList<BenchmarkRecord> records,
        IReadOnlyList<ScenarioDefinition> scenarios,
        BenchmarkEnvironment environment,
        string reportsDirectory,
        string rootDirectory)
    {
        ArgumentNullException.ThrowIfNull(records);
        ArgumentNullException.ThrowIfNull(scenarios);
        ArgumentNullException.ThrowIfNull(environment);
        ArgumentException.ThrowIfNullOrWhiteSpace(reportsDirectory);
        ArgumentException.ThrowIfNullOrWhiteSpace(rootDirectory);

        Directory.CreateDirectory(reportsDirectory);
        Directory.CreateDirectory(rootDirectory);

        var allCsv = Path.Combine(reportsDirectory, "benchmark_all.csv");
        var stackCsv = Path.Combine(reportsDirectory, "benchmark_stack.csv");
        var queueCsv = Path.Combine(reportsDirectory, "benchmark_queue.csv");
        var environmentMarkdown = Path.Combine(reportsDirectory, "benchmark_environment.md");
        var stackMarkdown = Path.Combine(reportsDirectory, "benchmark_stack.md");
        var queueMarkdown = Path.Combine(reportsDirectory, "benchmark_queue.md");
        var reportMarkdown = Path.Combine(reportsDirectory, "Report.md");
        var rootReportMarkdown = Path.Combine(rootDirectory, "Report.md");

        WriteCsv(allCsv, records);
        WriteCsv(stackCsv, records.Where(record => record.Family == "Stack"));
        WriteCsv(queueCsv, records.Where(record => record.Family == "Queue"));

        var environmentText = BuildEnvironmentMarkdown(environment);
        File.WriteAllText(environmentMarkdown, environmentText, new UTF8Encoding(false));

        var stackText = BuildFamilyMarkdown("Стек", records.Where(record => record.Family == "Stack"), scenarios, environment);
        File.WriteAllText(stackMarkdown, stackText, new UTF8Encoding(false));

        var queueText = BuildQueueMarkdown(records, scenarios, environment);
        File.WriteAllText(queueMarkdown, queueText, new UTF8Encoding(false));

        var report = BuildMarkdown(records, scenarios, environment);
        File.WriteAllText(reportMarkdown, report, new UTF8Encoding(false));
        File.WriteAllText(rootReportMarkdown, report, new UTF8Encoding(false));

        return new BenchmarkReportPaths(
            allCsv,
            stackCsv,
            queueCsv,
            environmentMarkdown,
            stackMarkdown,
            queueMarkdown,
            reportMarkdown,
            rootReportMarkdown);
    }

    public static string BuildMarkdown(
        IReadOnlyList<BenchmarkRecord> records,
        IReadOnlyList<ScenarioDefinition> scenarios,
        BenchmarkEnvironment environment)
    {
        var builder = new StringBuilder();
        builder.AppendLine("# Результаты экспериментов");
        builder.AppendLine();
        builder.AppendLine($"Отчёт сформирован автоматически {environment.TimestampUtc:yyyy-MM-dd HH:mm:ss} UTC.");
        builder.AppendLine("Все числа ниже — фактические результаты запуска программы, а не пример-заглушка.");
        builder.AppendLine();

        AppendEnvironment(builder, environment);
        AppendScenarios(builder, scenarios);
        AppendStackResults(builder, records);
        AppendQueueResults(builder, records);
        AppendQueueComparison(builder, records, scenarios);
        AppendAnalysis(builder, records, scenarios);
        AppendTheory(builder);
        AppendLimitations(builder);

        return builder.ToString();
    }

    private static void WriteCsv(string path, IEnumerable<BenchmarkRecord> records)
    {
        var builder = new StringBuilder();
        builder.AppendLine("Scenario,OperationCount,Composition,Implementation,Run,ReadMs,ExecutionMs,TotalMs,Repeats,Family,InputFile");
        foreach (var record in records)
        {
            builder.Append(Escape(record.Scenario)).Append(',')
                .Append(record.OperationCount.ToString(Invariant)).Append(',')
                .Append(Escape(record.Composition)).Append(',')
                .Append(Escape(record.Implementation)).Append(',')
                .Append(record.Run.ToString(Invariant)).Append(',')
                .Append(record.ReadMs.ToString("F4", Invariant)).Append(',')
                .Append(record.ExecutionMs.ToString("F4", Invariant)).Append(',')
                .Append(record.TotalMs.ToString("F4", Invariant)).Append(',')
                .Append(record.Repeats.ToString(Invariant)).Append(',')
                .Append(Escape(record.Family)).Append(',')
                .Append(Escape(record.InputFile)).AppendLine();
        }

        File.WriteAllText(path, builder.ToString(), new UTF8Encoding(false));
    }

    private static string Escape(string value)
    {
        if (!value.Contains(',') && !value.Contains('"') && !value.Contains('\n') && !value.Contains('\r'))
        {
            return value;
        }

        return "\"" + value.Replace("\"", "\"\"", StringComparison.Ordinal) + "\"";
    }

    private static void AppendEnvironment(StringBuilder builder, BenchmarkEnvironment environment)
    {
        builder.AppendLine("## Условия эксперимента");
        builder.AppendLine();
        builder.AppendLine("| Параметр | Значение |");
        builder.AppendLine("|---|---|");
        builder.AppendLine($"| Дата и время (UTC) | {environment.TimestampUtc:yyyy-MM-dd HH:mm:ss} |");
        builder.AppendLine($"| ОС | {Md(environment.OsDescription)} |");
        builder.AppendLine($"| Архитектура ОС / процесса | {environment.OsArchitecture} / {environment.ProcessArchitecture} |");
        builder.AppendLine($"| Платформа .NET | {Md(environment.FrameworkDescription)} |");
        builder.AppendLine($"| Версия среды | {Md(environment.RuntimeVersion)} |");
        builder.AppendLine($"| Конфигурация сборки | {environment.Configuration} |");
        builder.AppendLine($"| Логических процессоров | {environment.ProcessorCount} |");
        builder.AppendLine($"| Частота Stopwatch | {environment.StopwatchFrequency} тиков/с |");
        builder.AppendLine($"| Доступная память (GC) | {FormatBytes(environment.TotalAvailableMemoryBytes)} |");
        builder.AppendLine($"| Повторов на измерение | {environment.Repeats} |");
        builder.AppendLine($"| Прогревочных запусков | {environment.WarmupRuns} |");
        builder.AppendLine();
    }

    private static void AppendScenarios(StringBuilder builder, IReadOnlyList<ScenarioDefinition> scenarios)
    {
        builder.AppendLine("## Наборы входных данных");
        builder.AppendLine();
        builder.AppendLine("| Сценарий | Семейство | Операций | Состав | Файл |");
        builder.AppendLine("|---|---|---:|---|---|");
        foreach (var scenario in scenarios)
        {
            builder.AppendLine(
                $"| {scenario.Name} | {scenario.Family} | {scenario.OperationCount} | " +
                $"{Md(scenario.Composition)} | {scenario.FileName} |");
        }

        builder.AppendLine();
    }

    private static void AppendStackResults(StringBuilder builder, IReadOnlyList<BenchmarkRecord> records)
    {
        builder.AppendLine("## Стек на собственном связном списке");
        builder.AppendLine();
        builder.AppendLine("| Сценарий | Операций | Чтение+разбор, мс (среднее) | Выполнение, мс (среднее) | Медиана выполнения, мс | Мин/макс выполнения, мс | Полное время, мс (среднее) |");
        builder.AppendLine("|---|---:|---:|---:|---:|---:|---:|");
        foreach (var group in records
                     .Where(record => record.Family == "Stack")
                     .GroupBy(record => record.Scenario))
        {
            var read = BenchmarkStatistics.Summarize(group.Select(record => record.ReadMs));
            var execution = BenchmarkStatistics.Summarize(group.Select(record => record.ExecutionMs));
            var total = BenchmarkStatistics.Summarize(group.Select(record => record.TotalMs));
            builder.AppendLine(
                $"| {group.Key} | {group.First().OperationCount} | {F(read.Average)} | {F(execution.Average)} | " +
                $"{F(execution.Median)} | {F(execution.Min)} / {F(execution.Max)} | {F(total.Average)} |");
        }

        builder.AppendLine();
    }

    private static void AppendQueueResults(StringBuilder builder, IReadOnlyList<BenchmarkRecord> records)
    {
        builder.AppendLine("## Две реализации очереди");
        builder.AppendLine();
        builder.AppendLine("| Сценарий | Реализация | Операций | Чтение+разбор, мс (среднее) | Выполнение, мс (среднее) | Медиана выполнения, мс | Мин/макс выполнения, мс | Полное время, мс (среднее) |");
        builder.AppendLine("|---|---|---:|---:|---:|---:|---:|---:|");
        foreach (var group in records
                     .Where(record => record.Family == "Queue")
                     .GroupBy(record => (record.Scenario, record.Implementation)))
        {
            var read = BenchmarkStatistics.Summarize(group.Select(record => record.ReadMs));
            var execution = BenchmarkStatistics.Summarize(group.Select(record => record.ExecutionMs));
            var total = BenchmarkStatistics.Summarize(group.Select(record => record.TotalMs));
            builder.AppendLine(
                $"| {group.Key.Scenario} | {group.Key.Implementation} | {group.First().OperationCount} | " +
                $"{F(read.Average)} | {F(execution.Average)} | {F(execution.Median)} | " +
                $"{F(execution.Min)} / {F(execution.Max)} | {F(total.Average)} |");
        }

        builder.AppendLine();
    }

    private static void AppendQueueComparison(
        StringBuilder builder,
        IReadOnlyList<BenchmarkRecord> records,
        IReadOnlyList<ScenarioDefinition> scenarios)
    {
        builder.AppendLine("## Сравнение очередей на одинаковых файлах");
        builder.AppendLine();
        builder.AppendLine("| Сценарий | Операций | ListBasedQueue, мс | StandardQueue, мс | Отношение ListBased / Standard |");
        builder.AppendLine("|---|---:|---:|---:|---:|");
        foreach (var scenario in scenarios.Where(item => item.Family == "Queue"))
        {
            var list = records
                .Where(record => record.Scenario == scenario.Name && record.Implementation == "ListBasedQueue")
                .Select(record => record.ExecutionMs)
                .ToArray();
            var standard = records
                .Where(record => record.Scenario == scenario.Name && record.Implementation == "StandardQueue")
                .Select(record => record.ExecutionMs)
                .ToArray();

            if (list.Length == 0 || standard.Length == 0)
            {
                continue;
            }

            var listAverage = list.Average();
            var standardAverage = standard.Average();
            var ratio = standardAverage == 0 ? double.PositiveInfinity : listAverage / standardAverage;
            builder.AppendLine(
                $"| {scenario.Name} | {scenario.OperationCount} | {F(listAverage)} | {F(standardAverage)} | " +
                (double.IsFinite(ratio) ? ratio.ToString("F2", Invariant) + "×" : "н/д") + " |");
        }

        builder.AppendLine();
    }

    private static void AppendAnalysis(
        StringBuilder builder,
        IReadOnlyList<BenchmarkRecord> records,
        IReadOnlyList<ScenarioDefinition> scenarios)
    {
        builder.AppendLine("## Анализ результатов");
        builder.AppendLine();

        var queueScenarios = scenarios.Where(scenario => scenario.Family == "Queue").ToArray();
        var ratios = new List<(string Name, double Ratio)>();
        foreach (var scenario in queueScenarios)
        {
            var list = records
                .Where(record => record.Scenario == scenario.Name && record.Implementation == "ListBasedQueue")
                .Select(record => record.ExecutionMs)
                .ToArray();
            var standard = records
                .Where(record => record.Scenario == scenario.Name && record.Implementation == "StandardQueue")
                .Select(record => record.ExecutionMs)
                .ToArray();
            if (list.Length > 0 && standard.Length > 0 && standard.Average() > 0)
            {
                ratios.Add((scenario.Name, list.Average() / standard.Average()));
            }
        }

        var remove = ratios.FirstOrDefault(item => item.Name == "queue-remove-heavy");
        var enqueue = ratios.FirstOrDefault(item => item.Name == "queue-enqueue-heavy");
        var peek = ratios.FirstOrDefault(item => item.Name == "queue-peek-heavy");

        if (remove.Name is not null)
        {
            builder.AppendLine(
                $"- На сценарии `queue-remove-heavy` (массовое удаление из начала большой очереди) " +
                $"ListBasedQueue медленнее StandardQueue примерно в {F2(remove.Ratio)} раза. " +
                "Это качественно согласуется с Dequeue = List<T>.RemoveAt(0), требующим сдвига O(n) элементов.");
        }

        if (enqueue.Name is not null)
        {
            builder.AppendLine(
                $"- На сценарии `queue-enqueue-heavy` отношение составляет примерно {F2(enqueue.Ratio)}×: " +
                "при преобладании Enqueue обе реализации близки, поскольку добавление в конец List<T> амортизированно O(1).");
        }

        if (peek.Name is not null)
        {
            builder.AppendLine(
                $"- На сценарии `queue-peek-heavy` отношение примерно {F2(peek.Ratio)}×: просмотр начала у List<T> " +
                "индексируется за O(1), поэтому различия меньше, чем при массовом RemoveAt(0).");
        }

        var example = ratios.FirstOrDefault(item => item.Name == "queue-example");
        if (example.Name is not null)
        {
            builder.AppendLine(
                $"- Для `queue-example` (8 операций) отношение {F2(example.Ratio)}× не следует трактовать как " +
                "преимущество реализации: абсолютные времена порядка сотых долей миллисекунды, и на таком масштабе " +
                "доминируют накладные расходы и шум.");
        }

        var stackGroups = records.Where(record => record.Family == "Stack").GroupBy(record => record.Scenario).ToArray();
        if (stackGroups.Length > 0)
        {
            var largest = stackGroups
                .OrderByDescending(group => group.First().OperationCount)
                .First();
            var average = largest.Average(record => record.ExecutionMs);
            builder.AppendLine(
                $"- Для стека самый большой сценарий `{largest.Key}` ({largest.First().OperationCount} операций) " +
                $"выполнялся в среднем {F(average)} мс. Операции Push/Pop/Top/IsEmpty у связанного стека имеют O(1), " +
                "поэтому суммарное время ожидаемо линейно по числу команд; один сценарий сам по себе не доказывает асимптотику.");
        }

        builder.AppendLine(
            "- Абсолютные значения зависят от машины, JIT, сборщика мусора и фоновой нагрузки. " +
            "По одному измерению нельзя делать вывод об асимптотике; ниже сопоставлены теория и несколько повторов.");
        builder.AppendLine();
    }

    private static void AppendTheory(StringBuilder builder)
    {
        builder.AppendLine("## Теоретические оценки сложности");
        builder.AppendLine();
        builder.AppendLine("- Стек на односвязном списке: Push, Pop, Top, IsEmpty — O(1); память O(n).");
        builder.AppendLine("- ListBasedQueue: Enqueue — амортизированно O(1), Dequeue — O(n) из-за RemoveAt(0), Peek — O(1); память O(n).");
        builder.AppendLine("- StandardQueue: Enqueue, Dequeue, Peek — амортизированно O(1); память O(n).");
        builder.AppendLine("- Вычисление постфиксного выражения: O(m) времени и O(m) памяти в худшем случае, где m — число токенов.");
        builder.AppendLine("- Перевод инфиксной записи сортировочной станцией: O(n) времени и O(n) памяти.");
        builder.AppendLine("- Разворот списка, перенос конца/начала, удаление/вставка по известной позиции — O(n) из-за поиска; сами операции со ссылками O(1).");
        builder.AppendLine();
    }

    private static void AppendLimitations(StringBuilder builder)
    {
        builder.AppendLine("## Ограничения методики");
        builder.AppendLine();
        builder.AppendLine("- Время чтения включает чтение файла и разбор команд; время выполнения — только операции над структурой.");
        builder.AppendLine("- Вывод в консоль исключён из измеряемого участка: результаты операций не форматируются (observer = null).");
        builder.AppendLine("- Перед каждым измерением вызывается сборка мусора; это уменьшает перенос состояния между запусками, но не устраняет влияние ОС.");
        builder.AppendLine("- Для каждой пары реализаций очереди используется один и тот же файл сценария.");
        builder.AppendLine("- Замеры сделаны на одной машине и не являются универсальным бенчмарком.");
        builder.AppendLine();
    }

    private static string BuildFamilyMarkdown(
        string title,
        IEnumerable<BenchmarkRecord> records,
        IReadOnlyList<ScenarioDefinition> scenarios,
        BenchmarkEnvironment environment)
    {
        var builder = new StringBuilder();
        builder.AppendLine($"# {title}: результаты");
        builder.AppendLine();
        builder.AppendLine($"Сформировано {environment.TimestampUtc:yyyy-MM-dd HH:mm:ss} UTC; конфигурация {environment.Configuration}; повторов {environment.Repeats}.");
        builder.AppendLine();
        AppendScenarios(builder, scenarios.Where(scenario => scenario.Family == title switch
        {
            "Стек" => "Stack",
            _ => "Queue"
        }).ToArray());
        AppendStackResults(builder, records.ToArray());
        return builder.ToString();
    }

    private static string BuildQueueMarkdown(
        IReadOnlyList<BenchmarkRecord> records,
        IReadOnlyList<ScenarioDefinition> scenarios,
        BenchmarkEnvironment environment)
    {
        var builder = new StringBuilder();
        builder.AppendLine("# Очереди: результаты");
        builder.AppendLine();
        builder.AppendLine($"Сформировано {environment.TimestampUtc:yyyy-MM-dd HH:mm:ss} UTC; конфигурация {environment.Configuration}; повторов {environment.Repeats}.");
        builder.AppendLine();
        AppendScenarios(builder, scenarios.Where(scenario => scenario.Family == "Queue").ToArray());
        AppendQueueResults(builder, records);
        AppendQueueComparison(builder, records, scenarios);
        return builder.ToString();
    }

    private static string BuildEnvironmentMarkdown(BenchmarkEnvironment environment)
    {
        var builder = new StringBuilder();
        builder.AppendLine("# Условия эксперимента");
        builder.AppendLine();
        AppendEnvironment(builder, environment);
        return builder.ToString();
    }

    private static string F(double value) => value.ToString("F4", Invariant);

    private static string F2(double value) => value.ToString("F2", Invariant);

    private static string Md(string value) => value.Replace("|", "\\|", StringComparison.Ordinal);

    private static string FormatBytes(long bytes)
    {
        if (bytes <= 0)
        {
            return "не определено";
        }

        return (bytes / (1024.0 * 1024.0 * 1024.0)).ToString("F2", Invariant) + " ГиБ";
    }
}
