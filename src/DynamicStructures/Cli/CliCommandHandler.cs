namespace DynamicStructures.Cli;

public static class CliCommandHandler
{
    public static int Run(string[] args)
    {
        ArgumentNullException.ThrowIfNull(args);
        var output = Console.Out;

        try
        {
            if (args.Length == 0)
            {
                PrintHelp(output);
                return 0;
            }

            switch (args[0])
            {
                case "--help":
                case "-h":
                    PrintHelp(output);
                    return 0;
                case "--paths":
                    AppActions.PrintPaths(output);
                    return 0;
                case "--stack-file":
                    if (args.Length < 2)
                    {
                        output.WriteLine("Не указан путь после --stack-file.");
                        return 2;
                    }

                    AppActions.RunStackFile(args[1], output);
                    return 0;
                case "--queue-file":
                    if (args.Length < 2)
                    {
                        output.WriteLine("Не указан путь после --queue-file.");
                        return 2;
                    }

                    AppActions.RunQueueFile(
                        args[1],
                        standard: args.Contains("--standard", StringComparer.Ordinal),
                        output);
                    return 0;
                case "--postfix":
                    if (args.Length < 2)
                    {
                        output.WriteLine("Не указано постфиксное выражение.");
                        return 2;
                    }

                    AppActions.EvaluatePostfixExpression(string.Join(' ', args[1..]), output);
                    return 0;
                case "--infix":
                    if (args.Length < 2)
                    {
                        output.WriteLine("Не указано инфиксное выражение.");
                        return 2;
                    }

                    AppActions.ConvertInfixExpression(string.Join(' ', args[1..]), output);
                    return 0;
                case "--applications":
                    AppActions.RunApplications(output);
                    return 0;
                case "--list-demo":
                    AppActions.RunListDemo(output);
                    return 0;
                case "--generate-scenarios":
                    AppActions.GenerateScenarios(output);
                    return 0;
                case "--verify":
                    return AppActions.Verify(output) ? 0 : 1;
                case "--benchmark":
                    var repeats = 5;
                    if (args.Length >= 2 && int.TryParse(args[1], out var parsedRepeats))
                    {
                        repeats = parsedRepeats;
                    }

                    AppActions.RunBenchmarks(repeats, output);
                    return 0;
                default:
                    output.WriteLine($"Неизвестный аргумент: {args[0]}");
                    PrintHelp(output);
                    return 2;
            }
        }
        catch (Exception exception) when (exception is not OutOfMemoryException)
        {
            Console.Error.WriteLine($"Ошибка: {exception.Message}");
            return 1;
        }
    }

    private static void PrintHelp(TextWriter writer)
    {
        writer.WriteLine("Лабораторная работа: динамические структуры (C#).");
        writer.WriteLine();
        writer.WriteLine("Запуск без аргументов открывает интерактивное меню.");
        writer.WriteLine();
        writer.WriteLine("Режимы командной строки:");
        writer.WriteLine("  --stack-file <path>       выполнить команды стека из файла");
        writer.WriteLine("  --queue-file <path> [--standard]  выполнить команды очереди");
        writer.WriteLine("  --postfix <expr>          вычислить постфиксное выражение");
        writer.WriteLine("  --infix <expr>            преобразовать инфиксное выражение и вычислить");
        writer.WriteLine("  --applications            четыре примера применения структур");
        writer.WriteLine("  --list-demo               показать все 12 операций списка");
        writer.WriteLine("  --benchmark [repeats]     замеры и генерация отчётов");
        writer.WriteLine("  --generate-scenarios      перегенерировать файлы сценариев");
        writer.WriteLine("  --verify                  функциональная самопроверка");
        writer.WriteLine("  --paths                   показать пути проекта");
        writer.WriteLine("  --help                    эта справка");
        writer.WriteLine();
        writer.WriteLine("Примеры:");
        writer.WriteLine("  dotnet run --project src/DynamicStructures -- --stack-file input.txt");
        writer.WriteLine("  dotnet run --project src/DynamicStructures -- --postfix \"78+32+/\"");
        writer.WriteLine("  dotnet run --project src/DynamicStructures -- --infix \"sin(0)+sqrt(9)\"");
        writer.WriteLine("  dotnet run --project src/DynamicStructures -c Release -- --benchmark 5");
    }
}
