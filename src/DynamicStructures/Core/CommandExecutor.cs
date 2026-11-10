using System.Diagnostics.CodeAnalysis;
using DynamicStructures.DataStructures;

namespace DynamicStructures.Core;

public enum OperationStatus
{
    Ok,
    Empty,
    Invalid
}

public sealed record OperationOutcome(
    int Number,
    string Raw,
    CommandKind Kind,
    OperationStatus Status,
    string? Value = null,
    bool? BooleanValue = null,
    int? Count = null,
    IReadOnlyList<string>? Items = null,
    string? Error = null);

public interface IOperationObserver
{
    void OnOperation(OperationOutcome outcome);
}

public sealed class CollectingOperationObserver : IOperationObserver
{
    public List<OperationOutcome> Outcomes { get; } = new();

    public void OnOperation(OperationOutcome outcome) => Outcomes.Add(outcome);
}

/// <summary>
/// Выполняет команды над стеком или очередью. При observer == null не создаёт
/// объекты результатов и ничего не форматирует — это путь для замеров времени.
/// </summary>
public static class CommandExecutor
{
    private interface ITarget
    {
        int Count { get; }

        void Add(string value);

        bool TryRemove([MaybeNullWhen(false)] out string value);

        bool TryPeek([MaybeNullWhen(false)] out string value);

        bool IsEmpty { get; }

        string[] Snapshot();
    }

    private sealed class StackTarget(LinkedStack<string> stack) : ITarget
    {
        public int Count => stack.Count;

        public void Add(string value) => stack.Push(value);

        public bool TryRemove([MaybeNullWhen(false)] out string value) => stack.TryPop(out value);

        public bool TryPeek([MaybeNullWhen(false)] out string value) => stack.TryTop(out value);

        public bool IsEmpty => stack.IsEmpty;

        public string[] Snapshot() => stack.ToArrayTopFirst();
    }

    private sealed class QueueTarget(IQueue<string> queue) : ITarget
    {
        public int Count => queue.Count;

        public void Add(string value) => queue.Enqueue(value);

        public bool TryRemove([MaybeNullWhen(false)] out string value) => queue.TryDequeue(out value);

        public bool TryPeek([MaybeNullWhen(false)] out string value) => queue.TryPeek(out value);

        public bool IsEmpty => queue.IsEmpty;

        public string[] Snapshot() => queue.Snapshot();
    }

    public static void ExecuteStack(
        LinkedStack<string> stack,
        IEnumerable<Command> commands,
        IOperationObserver? observer = null)
    {
        ArgumentNullException.ThrowIfNull(stack);
        ExecuteCore(new StackTarget(stack), commands, observer, "стек", true);
    }

    public static void ExecuteQueue(
        IQueue<string> queue,
        IEnumerable<Command> commands,
        IOperationObserver? observer = null)
    {
        ArgumentNullException.ThrowIfNull(queue);
        ExecuteCore(new QueueTarget(queue), commands, observer, "очередь", false);
    }

    public static void WriteParseIssues(CommandFile file, TextWriter writer)
    {
        ArgumentNullException.ThrowIfNull(file);
        ArgumentNullException.ThrowIfNull(writer);

        foreach (var issue in file.Issues)
        {
            writer.WriteLine($"Ошибка разбора (строка {issue.Line}, токен '{issue.Token}'): {issue.Message}");
        }
    }

    private static void ExecuteCore(
        ITarget target,
        IEnumerable<Command> commands,
        IOperationObserver? observer,
        string structureLabel,
        bool isStack)
    {
        ArgumentNullException.ThrowIfNull(commands);

        if (observer is null)
        {
            ExecuteWithoutObservation(target, commands);
            return;
        }

        foreach (var command in commands)
        {
            OperationOutcome outcome;
            try
            {
                switch (command.Kind)
                {
                    case CommandKind.Push:
                        target.Add(command.Value!);
                        outcome = new OperationOutcome(
                            command.Number,
                            command.Raw,
                            command.Kind,
                            OperationStatus.Ok,
                            command.Value,
                            Count: target.Count);
                        break;
                    case CommandKind.Pop:
                        outcome = target.TryRemove(out var removed)
                            ? new OperationOutcome(
                                command.Number,
                                command.Raw,
                                command.Kind,
                                OperationStatus.Ok,
                                removed,
                                Count: target.Count)
                            : new OperationOutcome(
                                command.Number,
                                command.Raw,
                                command.Kind,
                                OperationStatus.Empty,
                                Count: 0);
                        break;
                    case CommandKind.Peek:
                        outcome = target.TryPeek(out var top)
                            ? new OperationOutcome(
                                command.Number,
                                command.Raw,
                                command.Kind,
                                OperationStatus.Ok,
                                top,
                                Count: target.Count)
                            : new OperationOutcome(
                                command.Number,
                                command.Raw,
                                command.Kind,
                                OperationStatus.Empty,
                                Count: 0);
                        break;
                    case CommandKind.IsEmpty:
                        outcome = new OperationOutcome(
                            command.Number,
                            command.Raw,
                            command.Kind,
                            OperationStatus.Ok,
                            BooleanValue: target.IsEmpty,
                            Count: target.Count);
                        break;
                    case CommandKind.Print:
                        outcome = new OperationOutcome(
                            command.Number,
                            command.Raw,
                            command.Kind,
                            OperationStatus.Ok,
                            Count: target.Count,
                            Items: target.Snapshot());
                        break;
                    default:
                        outcome = new OperationOutcome(
                            command.Number,
                            command.Raw,
                            command.Kind,
                            OperationStatus.Invalid,
                            Error: "Неизвестный тип команды.");
                        break;
                }
            }
            catch (Exception exception) when (exception is not OutOfMemoryException)
            {
                outcome = new OperationOutcome(
                    command.Number,
                    command.Raw,
                    command.Kind,
                    OperationStatus.Invalid,
                    Error: exception.Message);
            }

            observer.OnOperation(outcome);
        }
    }

    private static void ExecuteWithoutObservation(ITarget target, IEnumerable<Command> commands)
    {
        foreach (var command in commands)
        {
            switch (command.Kind)
            {
                case CommandKind.Push:
                    target.Add(command.Value!);
                    break;
                case CommandKind.Pop:
                    target.TryRemove(out _);
                    break;
                case CommandKind.Peek:
                    target.TryPeek(out _);
                    break;
                case CommandKind.IsEmpty:
                    _ = target.IsEmpty;
                    break;
                case CommandKind.Print:
                    _ = target.Snapshot();
                    break;
            }
        }
    }
}

/// <summary>Печатает результат каждой операции в заданный поток.</summary>
public sealed class ConsoleOperationObserver : IOperationObserver
{
    private readonly TextWriter _writer;
    private readonly string _structureLabel;
    private readonly bool _isStack;

    public ConsoleOperationObserver(TextWriter writer, string structureLabel, bool isStack)
    {
        _writer = writer ?? throw new ArgumentNullException(nameof(writer));
        _structureLabel = structureLabel;
        _isStack = isStack;
    }

    public void OnOperation(OperationOutcome outcome)
    {
        _writer.WriteLine(Format(outcome, _structureLabel, _isStack));
    }

    public static string Format(OperationOutcome outcome, string structureLabel, bool isStack)
    {
        var prefix = $"[{outcome.Number}] {outcome.Raw} -> ";
        if (outcome.Status == OperationStatus.Invalid)
        {
            return prefix + "ошибка: " + outcome.Error;
        }

        var emptyWord = structureLabel == "стек" ? "пуст" : "пуста";

        return outcome.Kind switch
        {
            CommandKind.Push =>
                prefix + $"добавлено \"{outcome.Value}\"; размер = {outcome.Count}",
            CommandKind.Pop when outcome.Status == OperationStatus.Empty =>
                prefix + $"{structureLabel} {emptyWord}: удаление пропущено",
            CommandKind.Pop =>
                prefix + $"удалено \"{outcome.Value}\"; размер = {outcome.Count}",
            CommandKind.Peek when outcome.Status == OperationStatus.Empty =>
                prefix + $"{structureLabel} {emptyWord}: просмотр невозможен",
            CommandKind.Peek =>
                prefix + $"верх/начало: \"{outcome.Value}\"; размер = {outcome.Count}",
            CommandKind.IsEmpty =>
                prefix + $"пусто: {(outcome.BooleanValue == true ? "да" : "нет")}; размер = {outcome.Count}",
            CommandKind.Print =>
                prefix +
                (isStack ? "стек (сверху вниз): " : "очередь (начало -> конец): ") +
                (outcome.Items is null || outcome.Items.Count == 0
                    ? "(пусто)"
                    : string.Join(" ", outcome.Items)),
            _ => prefix + "неизвестная команда"
        };
    }
}
