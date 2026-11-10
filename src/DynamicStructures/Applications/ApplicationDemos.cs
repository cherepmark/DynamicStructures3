namespace DynamicStructures.Applications;

/// <summary>Четыре демонстрационных примера практического применения структур.</summary>
public static class ApplicationDemos
{
    public static void RunAll(TextWriter writer)
    {
        ArgumentNullException.ThrowIfNull(writer);

        RunPlaylistDemo(writer);
        writer.WriteLine();
        RunEditorHistoryDemo(writer);
        writer.WriteLine();
        RunPrintQueueDemo(writer);
        writer.WriteLine();
        RunExpressionTreeDemo(writer);
    }

    public static void RunPlaylistDemo(TextWriter writer)
    {
        writer.WriteLine("=== 1. Список: плейлист ===");
        writer.WriteLine("Связный список удобен для частых вставок и удалений в середине: ссылки меняются локально.");

        var playlist = new Playlist("Дорога");
        playlist.Add(new Song("Highway Star", "Deep Purple", 372));
        playlist.Add(new Song("Paranoid", "Black Sabbath", 168));
        playlist.Add(new Song("The Trooper", "Iron Maiden", 252));
        playlist.Add(new Song("Run to the Hills", "Iron Maiden", 234));

        writer.WriteLine("Исходный порядок:");
        playlist.WriteTo(writer);

        var moved = playlist[0];
        playlist.Move(0, 2);
        writer.WriteLine($"После перемещения «{moved.Title}» на позицию 3:");
        playlist.WriteTo(writer);

        var removed = playlist[2];
        playlist.Remove(removed.Title);
        writer.WriteLine($"После удаления «{removed.Title}»:");
        playlist.WriteTo(writer);
        writer.WriteLine($"Всего песен: {playlist.Count}; длительность: {playlist.TotalDuration:mm\\:ss}.");
    }

    public static void RunEditorHistoryDemo(TextWriter writer)
    {
        writer.WriteLine("=== 2. Стек: история действий редактора ===");
        writer.WriteLine("Стек хранит предыдущие состояния: отмена всегда снимает последнее действие (LIFO).");

        var history = new EditorHistory();
        history.Apply("Привет");
        history.Apply("Привет, мир");
        history.Apply("Привет, мир!");
        writer.WriteLine($"Текущий текст: \"{history.Current}\"");
        writer.WriteLine($"Глубина истории: {history.UndoDepth}");

        history.Undo();
        writer.WriteLine($"После одного Undo: \"{history.Current}\"");
        history.Undo();
        writer.WriteLine($"После второго Undo: \"{history.Current}\"");
        history.Undo();
        writer.WriteLine($"После третьего Undo: \"{history.Current}\"");

        var emptyUndo = history.Undo();
        writer.WriteLine($"Четвёртый Undo успешен: {emptyUndo}; текущий текст: \"{history.Current}\"");
    }

    public static void RunPrintQueueDemo(TextWriter writer)
    {
        writer.WriteLine("=== 3. Очередь: задания на печать ===");
        writer.WriteLine("Очередь гарантирует порядок поступления заданий (FIFO).");

        var printer = new PrintQueueSimulation();
        printer.Submit(new PrintJob("Отчёт.docx", 12));
        printer.Submit(new PrintJob("Схема.pdf", 3));
        printer.Submit(new PrintJob("Приказ.pdf", 2));

        writer.WriteLine($"Очередь ({printer.ImplementationName}):");
        printer.WritePending(writer);

        while (printer.TryProcessNext(out var job))
        {
            writer.WriteLine($"Напечатан документ: {job.Document} ({job.Pages} стр.)");
        }

        writer.WriteLine($"Осталось заданий: {printer.PendingCount}; пусто: {printer.IsEmpty}.");
    }

    public static void RunExpressionTreeDemo(TextWriter writer)
    {
        writer.WriteLine("=== 4. Дерево: арифметическое выражение ===");
        writer.WriteLine("Дерево хранит структуру выражения; обходы дают разные формы записи.");

        var tree = ExpressionTree.FromPostfix("2 3 + 4 *");
        writer.WriteLine("Постфиксная запись: 2 3 + 4 *");
        writer.WriteLine($"Инфиксный обход: {tree.Inorder()}");
        writer.WriteLine($"Префиксный обход: {tree.Preorder()}");
        writer.WriteLine($"Постфиксный обход: {tree.Postorder()}");
        writer.WriteLine($"Значение: {tree.Evaluate()}");

        var functionTree = ExpressionTree.FromInfix("sin(0)+sqrt(9)");
        writer.WriteLine($"Выражение sin(0)+sqrt(9): инфикс {functionTree.Inorder()}, значение {functionTree.Evaluate()}.");
    }
}
