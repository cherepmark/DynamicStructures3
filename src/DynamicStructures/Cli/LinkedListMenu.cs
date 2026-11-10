using DynamicStructures.Core;
using DynamicStructures.DataStructures;
using DynamicStructures.Demo;
using DynamicStructures.Infrastructure;
using DynamicStructures.LinkedListTasks;

namespace DynamicStructures.Cli;

public sealed class LinkedListMenu
{
    private readonly TextReader _input;
    private readonly TextWriter _output;
    private SinglyLinkedList<int> _current = new();
    private SinglyLinkedList<int> _other = new();

    public LinkedListMenu(TextReader input, TextWriter output)
    {
        _input = input ?? throw new ArgumentNullException(nameof(input));
        _output = output ?? throw new ArgumentNullException(nameof(output));
    }

    public void Run()
    {
        TryLoadDefaultLists();

        while (true)
        {
            _output.WriteLine();
            _output.WriteLine("--- Связный список: 12 операций ---");
            PrintLists();
            _output.WriteLine("1.  Загрузить L из файла");
            _output.WriteLine("2.  Загрузить E из файла");
            _output.WriteLine("3.  Ввести L вручную");
            _output.WriteLine("4.  Ввести E вручную");
            _output.WriteLine("5.  Показать списки");
            _output.WriteLine("6.  Развернуть L");
            _output.WriteLine("7.  Перенести последний элемент L в начало");
            _output.WriteLine("8.  Перенести первый элемент L в конец");
            _output.WriteLine("9.  Количество различных элементов L");
            _output.WriteLine("10. Удалить неуникальные элементы L");
            _output.WriteLine("11. Вставить копию L после первого вхождения x");
            _output.WriteLine("12. Вставить E в упорядоченный L");
            _output.WriteLine("13. Удалить все вхождения E из L");
            _output.WriteLine("14. Вставить F перед первым вхождением E");
            _output.WriteLine("15. Дописать E к L (копии узлов)");
            _output.WriteLine("16. Разделить L по первому вхождению значения");
            _output.WriteLine("17. Удвоить L");
            _output.WriteLine("18. Обменять два элемента L по позициям");
            _output.WriteLine("19. Показать демонстрацию всех 12 операций");
            _output.WriteLine("0.  Назад");

            var choice = Prompt("Выбор");
            try
            {
                switch (choice)
                {
                    case "1":
                        LoadList(isCurrent: true);
                        break;
                    case "2":
                        LoadList(isCurrent: false);
                        break;
                    case "3":
                        EnterList(isCurrent: true);
                        break;
                    case "4":
                        EnterList(isCurrent: false);
                        break;
                    case "5":
                        PrintLists();
                        break;
                    case "6":
                        LinkedListAlgorithms.Reverse(_current);
                        PrintCurrent("Разворот выполнен");
                        break;
                    case "7":
                        LinkedListAlgorithms.MoveLastToFront(_current);
                        PrintCurrent("Последний элемент перенесён в начало");
                        break;
                    case "8":
                        LinkedListAlgorithms.MoveFirstToLast(_current);
                        PrintCurrent("Первый элемент перенесён в конец");
                        break;
                    case "9":
                        _output.WriteLine($"Различных элементов: {LinkedListAlgorithms.CountDistinct(_current)}");
                        break;
                    case "10":
                        var removed = LinkedListAlgorithms.RemoveNonUnique(_current);
                        PrintCurrent($"Удалено вхождений неуникальных значений: {removed}");
                        break;
                    case "11":
                        if (TryReadInt("Значение x", out var marker))
                        {
                            var ok = LinkedListAlgorithms.InsertCopyAfterFirst(_current, marker);
                            PrintCurrent(ok
                                ? "Копия списка вставлена после первого вхождения"
                                : "Значение x не найдено, список не изменён");
                        }

                        break;
                    case "12":
                        if (!IsNonDecreasing(_current))
                        {
                            _output.WriteLine("Список L не отсортирован по неубыванию.");
                            break;
                        }

                        if (TryReadInt("Значение E", out var sortedValue))
                        {
                            LinkedListAlgorithms.InsertIntoSorted(_current, sortedValue);
                            PrintCurrent("Вставка в упорядоченный список выполнена");
                        }

                        break;
                    case "13":
                        if (TryReadInt("Значение E", out var removeValue))
                        {
                            var count = LinkedListAlgorithms.RemoveAll(_current, removeValue);
                            PrintCurrent($"Удалено элементов: {count}");
                        }

                        break;
                    case "14":
                        if (TryReadInt("Значение E (перед ним вставляем)", out var target) &&
                            TryReadInt("Значение F (что вставляем)", out var inserted))
                        {
                            var ok = LinkedListAlgorithms.InsertBeforeFirst(_current, target, inserted);
                            PrintCurrent(ok
                                ? "Вставка выполнена"
                                : "Значение E не найдено, список не изменён");
                        }

                        break;
                    case "15":
                        LinkedListAlgorithms.Concatenate(_current, _other);
                        PrintLists("E дописан к L независимыми копиями");
                        break;
                    case "16":
                        if (TryReadInt("Значение для разделения", out var splitValue))
                        {
                            var (first, second) = LinkedListAlgorithms.SplitAtFirst(_current, splitValue);
                            _current = first;
                            _other = second;
                            PrintLists("L разделён: First -> L, Second -> E");
                        }

                        break;
                    case "17":
                        LinkedListAlgorithms.Duplicate(_current);
                        PrintCurrent("Список удвоен");
                        break;
                    case "18":
                        if (TryReadInt("Первая позиция (1-based)", out var firstPosition) &&
                            TryReadInt("Вторая позиция (1-based)", out var secondPosition))
                        {
                            LinkedListAlgorithms.SwapAt(_current, firstPosition - 1, secondPosition - 1);
                            PrintCurrent("Элементы обменяны");
                        }

                        break;
                    case "19":
                        LinkedListDemo.RunAll(_output);
                        break;
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

    private void TryLoadDefaultLists()
    {
        try
        {
            var path = ProjectPaths.ResolveExistingFile("data/list_L.txt", "list_L.txt");
            _current = IntListFile.Load(path);
        }
        catch (FileNotFoundException)
        {
            _current = new SinglyLinkedList<int>();
        }

        try
        {
            var path = ProjectPaths.ResolveExistingFile("data/list_E.txt", "list_E.txt");
            _other = IntListFile.Load(path);
        }
        catch (FileNotFoundException)
        {
            _other = new SinglyLinkedList<int>();
        }
    }

    private void LoadList(bool isCurrent)
    {
        var defaultFile = isCurrent ? "data/list_L.txt" : "data/list_E.txt";
        var pathInput = Prompt($"Путь к файлу (Enter — {defaultFile})");
        var path = string.IsNullOrWhiteSpace(pathInput) ? defaultFile : pathInput;
        var resolved = ProjectPaths.ResolveExistingFile(path, defaultFile);
        var loaded = IntListFile.Load(resolved);

        if (isCurrent)
        {
            _current = loaded;
        }
        else
        {
            _other = loaded;
        }

        PrintLists($"Загружен {resolved}");
    }

    private void EnterList(bool isCurrent)
    {
        _output.Write("Целые числа через пробел/запятую (пустая строка — пустой список): ");
        var line = _input.ReadLine() ?? string.Empty;
        try
        {
            var parsed = IntListFile.Parse(line, "<ввод>");
            if (isCurrent)
            {
                _current = parsed;
            }
            else
            {
                _other = parsed;
            }

            PrintLists("Список введён");
        }
        catch (FormatException exception)
        {
            _output.WriteLine($"Ошибка: {exception.Message}");
        }
    }

    private void PrintLists(string? message = null)
    {
        if (!string.IsNullOrEmpty(message))
        {
            _output.WriteLine(message);
        }

        _output.WriteLine(
            $"L ({_current.Count}, цикл={_current.HasCycle()}): {Format(_current)}");
        _output.WriteLine(
            $"E ({_other.Count}, цикл={_other.HasCycle()}): {Format(_other)}");
    }

    private void PrintCurrent(string message)
    {
        _output.WriteLine(message);
        _output.WriteLine($"L ({_current.Count}, цикл={_current.HasCycle()}): {Format(_current)}");
    }

    private string Prompt(string text)
    {
        _output.Write(text + ": ");
        return (_input.ReadLine() ?? string.Empty).Trim();
    }

    private bool TryReadInt(string text, out int value)
    {
        var raw = Prompt(text);
        if (int.TryParse(raw, out value))
        {
            return true;
        }

        _output.WriteLine("Ожидалось целое число.");
        return false;
    }

    private static bool IsNonDecreasing(SinglyLinkedList<int> list)
    {
        var previous = int.MinValue;
        foreach (var value in list.Items())
        {
            if (value < previous)
            {
                return false;
            }

            previous = value;
        }

        return true;
    }

    private static string Format<T>(SinglyLinkedList<T> list) =>
        "[" + string.Join(" ", list.Items()) + "]";
}
