using DynamicStructures.Core;
using DynamicStructures.DataStructures;
using DynamicStructures.Infrastructure;
using DynamicStructures.Trees;

namespace DynamicStructures.Cli;

public sealed class ConsoleMenu
{
    private readonly TextReader _input;
    private readonly TextWriter _output;

    public ConsoleMenu(TextReader input, TextWriter output)
    {
        _input = input ?? throw new ArgumentNullException(nameof(input));
        _output = output ?? throw new ArgumentNullException(nameof(output));
    }

    public void Run()
    {
        _output.WriteLine("Практическое задание № 3: Динамические структуры");
        _output.WriteLine($"Корень проекта: {ProjectPaths.RepositoryRoot}");

        while (true)
        {
            _output.WriteLine();
            _output.WriteLine("=== Главное меню ===");
            _output.WriteLine("1.  Стек: выполнить input.txt");
            _output.WriteLine("2.  Стек: выполнить указанный файл команд");
            _output.WriteLine("3.  Стек: ввести команды вручную");
            _output.WriteLine("4.  Стек: замерить время input.txt");
            _output.WriteLine("5.  Очередь ListBasedQueue: выполнить input.txt");
            _output.WriteLine("6.  Очередь ListBasedQueue: выполнить указанный файл");
            _output.WriteLine("7.  Очередь StandardQueue: выполнить input.txt");
            _output.WriteLine("8.  Очередь StandardQueue: выполнить указанный файл");
            _output.WriteLine("9.  Очередь: ввести команды вручную");
            _output.WriteLine("10. Постфиксное выражение: ввести вручную");
            _output.WriteLine("11. Постфиксные выражения из файла");
            _output.WriteLine("12. Инфиксное выражение: преобразовать и вычислить");
            _output.WriteLine("13. Инфиксные выражения из файла");
            _output.WriteLine("14. Часть 4: 12 операций со связным списком (интерактивно)");
            _output.WriteLine("15. Показать демонстрацию всех 12 операций");
            _output.WriteLine("16. Примеры практического применения (4 шт.)");
            _output.WriteLine("17. Сравнительные бенчмарки стека и очередей");
            _output.WriteLine("18. Перегенерировать файлы сценариев");
            _output.WriteLine("19. Показать пути проекта");
            _output.WriteLine("20. Бинарное дерево из префиксной записи");
            _output.WriteLine("0.  Выход");

            var choice = Prompt("Выбор");
            try
            {
                switch (choice)
                {
                    case "1":
                        AppActions.RunStackFile("input.txt", _output);
                        break;
                    case "2":
                        AppActions.RunStackFile(Prompt("Путь к файлу"), _output);
                        break;
                    case "3":
                        RunManualCommands(isStack: true, standardQueue: false);
                        break;
                    case "4":
                        AppActions.MeasureStackFile("input.txt", ReadRepeats(5), _output);
                        break;
                    case "5":
                        AppActions.RunQueueFile("input.txt", standard: false, _output);
                        break;
                    case "6":
                        AppActions.RunQueueFile(Prompt("Путь к файлу"), standard: false, _output);
                        break;
                    case "7":
                        AppActions.RunQueueFile("input.txt", standard: true, _output);
                        break;
                    case "8":
                        AppActions.RunQueueFile(Prompt("Путь к файлу"), standard: true, _output);
                        break;
                    case "9":
                        RunManualCommands(isStack: false, standardQueue: AskStandardQueue());
                        break;
                    case "10":
                        AppActions.EvaluatePostfixExpression(Prompt("Постфиксное выражение"), _output);
                        break;
                    case "11":
                        AppActions.EvaluatePostfixFile(Prompt("Путь к файлу (Enter — data/postfix.txt)"), _output);
                        break;
                    case "12":
                        AppActions.ConvertInfixExpression(Prompt("Инфиксное выражение"), _output);
                        break;
                    case "13":
                        AppActions.ConvertInfixFile(Prompt("Путь к файлу (Enter — data/infix.txt)"), _output);
                        break;
                    case "14":
                        new LinkedListMenu(_input, _output).Run();
                        break;
                    case "15":
                        AppActions.RunListDemo(_output);
                        break;
                    case "16":
                        AppActions.RunApplications(_output);
                        break;
                    case "17":
                        AppActions.RunBenchmarks(ReadRepeats(5), _output);
                        break;
                    case "18":
                        AppActions.GenerateScenarios(_output);
                        break;
                    case "19":
                        AppActions.PrintPaths(_output);
                        break;
                    case "20":
                    {
                        var prefix = Prompt($"Префиксная запись (Enter — {TreeBuilder.DefaultPrefix})");
                        AppActions.RunTreeBuilder(
                            string.IsNullOrWhiteSpace(prefix) ? null : prefix,
                            _output);
                        break;
                    }
                    case "0":
                        return;
                    default:
                        _output.WriteLine("Неизвестный пункт меню.");
                        break;
                }
            }
            catch (Exception exception) when (exception is not OutOfMemoryException)
            {
                _output.WriteLine($"Ошибка: {exception.Message}");
            }
        }
    }

    private void RunManualCommands(bool isStack, bool standardQueue)
    {
        _output.Write("Команды через пробел (например, 3 4 1,56 1,7 1,cat 2 5 4): ");
        var text = _input.ReadLine() ?? string.Empty;
        var file = CommandParser.Parse(text, "<ручной ввод>");
        CommandExecutor.WriteParseIssues(file, _output);

        if (isStack)
        {
            var stack = new LinkedStack<string>();
            CommandExecutor.ExecuteStack(
                stack,
                file.Commands,
                new ConsoleOperationObserver(_output, "стек", isStack: true));
            _output.WriteLine($"Итог: размер = {stack.Count}; пусто = {stack.IsEmpty}.");
        }
        else
        {
            IQueue<string> queue = standardQueue ? new StandardQueue<string>() : new ListBasedQueue<string>();
            CommandExecutor.ExecuteQueue(
                queue,
                file.Commands,
                new ConsoleOperationObserver(_output, "очередь", isStack: false));
            _output.WriteLine($"Итог ({queue.ImplementationName}): размер = {queue.Count}; пусто = {queue.IsEmpty}.");
        }
    }

    private bool AskStandardQueue()
    {
        while (true)
        {
            var answer = Prompt("Реализация: 1 — ListBasedQueue, 2 — StandardQueue");
            switch (answer)
            {
                case "1":
                    return false;
                case "2":
                    return true;
                default:
                    _output.WriteLine("Введите 1 или 2.");
                    break;
            }
        }
    }

    private int ReadRepeats(int defaultValue)
    {
        var raw = Prompt($"Количество повторов (Enter — {defaultValue})");
        if (string.IsNullOrWhiteSpace(raw))
        {
            return defaultValue;
        }

        if (int.TryParse(raw, out var repeats) && repeats is >= 1 and <= 100)
        {
            return repeats;
        }

        _output.WriteLine($"Некорректное число, используется {defaultValue}.");
        return defaultValue;
    }

    private string Prompt(string text)
    {
        _output.Write(text + ": ");
        return (_input.ReadLine() ?? string.Empty).Trim();
    }
}
