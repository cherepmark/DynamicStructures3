using System.Globalization;
using System.Text;
using DynamicStructures.Core;

namespace DynamicStructures.Benchmarking;

public sealed record ScenarioDefinition(
    string Name,
    string Family,
    string FileName,
    string FilePath,
    int OperationCount,
    string Composition,
    string Description);

/// <summary>
/// Генерирует корректные сценарии команд. Для сценариев очереди удаление и
/// просмотр выполняются только при непустой очереди, чтобы сравнение не
/// искажалось холостыми операциями.
/// </summary>
public static class ScenarioGenerator
{
    public static IReadOnlyList<ScenarioDefinition> WriteAll(string scenariosDirectory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(scenariosDirectory);
        Directory.CreateDirectory(scenariosDirectory);

        var scenarios = new List<ScenarioDefinition>
        {
            BuildStackExample(scenariosDirectory),
            BuildStackShort(scenariosDirectory),
            BuildStackMedium(scenariosDirectory),
            BuildStackLarge(scenariosDirectory),
            BuildStackPushHeavy(scenariosDirectory),
            BuildStackPopHeavy(scenariosDirectory),
            BuildStackPeekHeavy(scenariosDirectory),
            BuildStackMixed(scenariosDirectory),
            BuildQueueExample(scenariosDirectory),
            BuildQueueShort(scenariosDirectory),
            BuildQueueMedium(scenariosDirectory),
            BuildQueueLarge(scenariosDirectory),
            BuildQueueRemoveHeavy(scenariosDirectory),
            BuildQueueEnqueueHeavy(scenariosDirectory),
            BuildQueuePeekHeavy(scenariosDirectory),
            BuildQueueMixed(scenariosDirectory)
        };

        return scenarios;
    }

    private static ScenarioDefinition BuildStackExample(string directory)
    {
        var builder = new ScenarioBuilder();
        builder.Push();
        builder.Push();
        builder.Push();
        builder.IsEmpty();
        builder.Peek();
        builder.Pop();
        builder.Print();
        builder.IsEmpty();
        return builder.Finish(
            "stack-example",
            "Stack",
            "stack_example.txt",
            directory,
            "Короткий корректный пример операций стека.");
    }

    private static ScenarioDefinition BuildStackShort(string directory)
    {
        var builder = new ScenarioBuilder();
        for (var i = 0; i < 300; i++)
        {
            builder.Push();
        }

        for (var i = 0; i < 200; i++)
        {
            builder.Peek();
            builder.IsEmpty();
        }

        for (var i = 0; i < 300; i++)
        {
            builder.Pop();
        }

        return builder.Finish("stack-short", "Stack", "stack_short.txt", directory,
            "1000 операций: 300 push, 400 peek/isEmpty, 300 pop.");
    }

    private static ScenarioDefinition BuildStackMedium(string directory)
    {
        var builder = new ScenarioBuilder();
        for (var i = 0; i < 4_000; i++)
        {
            builder.Push();
        }

        for (var i = 0; i < 2_000; i++)
        {
            builder.Peek();
            builder.IsEmpty();
        }

        for (var i = 0; i < 2_000; i++)
        {
            builder.Pop();
        }

        return builder.Finish("stack-medium", "Stack", "stack_medium.txt", directory,
            "10 000 операций: 4000 push, 4000 peek/isEmpty, 2000 pop.");
    }

    private static ScenarioDefinition BuildStackLarge(string directory)
    {
        var builder = new ScenarioBuilder();
        for (var i = 0; i < 50_000; i++)
        {
            builder.Push();
        }

        for (var i = 0; i < 20_000; i++)
        {
            builder.Peek();
        }

        for (var i = 0; i < 10_000; i++)
        {
            builder.IsEmpty();
        }

        for (var i = 0; i < 20_000; i++)
        {
            builder.Pop();
        }

        return builder.Finish("stack-large", "Stack", "stack_large.txt", directory,
            "100 000 операций: 50 000 push, 20 000 peek, 10 000 isEmpty, 20 000 pop.");
    }

    private static ScenarioDefinition BuildStackPushHeavy(string directory)
    {
        var builder = new ScenarioBuilder();
        for (var i = 0; i < 90_000; i++)
        {
            builder.Push();
        }

        for (var i = 0; i < 10_000; i++)
        {
            builder.IsEmpty();
        }

        return builder.Finish("stack-push-heavy", "Stack", "stack_push_heavy.txt", directory,
            "100 000 операций: 90 000 push и 10 000 isEmpty.");
    }

    private static ScenarioDefinition BuildStackPopHeavy(string directory)
    {
        var builder = new ScenarioBuilder();
        for (var i = 0; i < 50_000; i++)
        {
            builder.Push();
        }

        for (var i = 0; i < 50_000; i++)
        {
            builder.Pop();
        }

        return builder.Finish("stack-pop-heavy", "Stack", "stack_pop_heavy.txt", directory,
            "100 000 операций: 50 000 push и 50 000 pop.");
    }

    private static ScenarioDefinition BuildStackPeekHeavy(string directory)
    {
        var builder = new ScenarioBuilder();
        for (var i = 0; i < 40_000; i++)
        {
            builder.Push();
        }

        for (var i = 0; i < 40_000; i++)
        {
            builder.Peek();
        }

        for (var i = 0; i < 10_000; i++)
        {
            builder.IsEmpty();
        }

        for (var i = 0; i < 10_000; i++)
        {
            builder.Pop();
        }

        return builder.Finish("stack-peek-heavy", "Stack", "stack_peek_heavy.txt", directory,
            "100 000 операций: 40 000 push, 40 000 peek, 10 000 isEmpty, 10 000 pop.");
    }

    private static ScenarioDefinition BuildStackMixed(string directory)
    {
        var builder = new ScenarioBuilder();
        for (var i = 0; i < 40_000; i++)
        {
            builder.Push();
        }

        for (var i = 0; i < 20_000; i++)
        {
            builder.Peek();
        }

        for (var i = 0; i < 10_000; i++)
        {
            builder.IsEmpty();
        }

        for (var i = 0; i < 20_000; i++)
        {
            builder.Pop();
        }

        for (var i = 0; i < 9_999; i++)
        {
            builder.Push();
        }

        builder.Print();
        return builder.Finish("stack-mixed", "Stack", "stack_mixed.txt", directory,
            "100 000 операций: смесь push/peek/isEmpty/pop и одно print.");
    }

    private static ScenarioDefinition BuildQueueExample(string directory)
    {
        var builder = new ScenarioBuilder();
        builder.Push();
        builder.Push();
        builder.Push();
        builder.IsEmpty();
        builder.Peek();
        builder.Pop();
        builder.Print();
        builder.IsEmpty();
        return builder.Finish(
            "queue-example",
            "Queue",
            "queue_example.txt",
            directory,
            "Короткий корректный пример операций очереди.");
    }

    private static ScenarioDefinition BuildQueueShort(string directory)
    {
        var builder = new ScenarioBuilder();
        for (var i = 0; i < 400; i++)
        {
            builder.Push();
        }

        for (var i = 0; i < 300; i++)
        {
            builder.Pop();
        }

        for (var i = 0; i < 200; i++)
        {
            builder.Push();
        }

        for (var i = 0; i < 100; i++)
        {
            builder.Peek();
        }

        return builder.Finish("queue-short", "Queue", "queue_short.txt", directory,
            "1000 операций: 600 enqueue, 300 dequeue, 100 peek.");
    }

    private static ScenarioDefinition BuildQueueMedium(string directory)
    {
        var builder = new ScenarioBuilder();
        for (var i = 0; i < 4_000; i++)
        {
            builder.Push();
        }

        for (var i = 0; i < 2_000; i++)
        {
            builder.Peek();
            builder.IsEmpty();
        }

        for (var i = 0; i < 2_000; i++)
        {
            builder.Pop();
        }

        return builder.Finish("queue-medium", "Queue", "queue_medium.txt", directory,
            "10 000 операций: 4000 enqueue, 4000 peek/isEmpty, 2000 dequeue.");
    }

    private static ScenarioDefinition BuildQueueLarge(string directory)
    {
        var builder = new ScenarioBuilder();
        for (var i = 0; i < 30_000; i++)
        {
            builder.Push();
        }

        for (var i = 0; i < 10_000; i++)
        {
            builder.Pop();
        }

        return builder.Finish("queue-large", "Queue", "queue_large.txt", directory,
            "40 000 операций: 30 000 enqueue и 10 000 dequeue при большой очереди.");
    }

    private static ScenarioDefinition BuildQueueRemoveHeavy(string directory)
    {
        var builder = new ScenarioBuilder();
        for (var i = 0; i < 20_000; i++)
        {
            builder.Push();
        }

        for (var i = 0; i < 20_000; i++)
        {
            builder.Pop();
        }

        return builder.Finish("queue-remove-heavy", "Queue", "queue_remove_heavy.txt", directory,
            "40 000 операций: 20 000 enqueue и 20 000 dequeue из начала большой очереди.");
    }

    private static ScenarioDefinition BuildQueueEnqueueHeavy(string directory)
    {
        var builder = new ScenarioBuilder();
        for (var i = 0; i < 38_000; i++)
        {
            builder.Push();
        }

        for (var i = 0; i < 1_000; i++)
        {
            builder.Peek();
        }

        for (var i = 0; i < 1_000; i++)
        {
            builder.IsEmpty();
        }

        return builder.Finish("queue-enqueue-heavy", "Queue", "queue_enqueue_heavy.txt", directory,
            "40 000 операций: 38 000 enqueue, 1000 peek, 1000 isEmpty.");
    }

    private static ScenarioDefinition BuildQueuePeekHeavy(string directory)
    {
        var builder = new ScenarioBuilder();
        for (var i = 0; i < 10_000; i++)
        {
            builder.Push();
        }

        for (var i = 0; i < 25_000; i++)
        {
            builder.Peek();
        }

        for (var i = 0; i < 4_000; i++)
        {
            builder.IsEmpty();
        }

        for (var i = 0; i < 1_000; i++)
        {
            builder.Pop();
        }

        return builder.Finish("queue-peek-heavy", "Queue", "queue_peek_heavy.txt", directory,
            "40 000 операций: 10 000 enqueue, 25 000 peek, 4000 isEmpty, 1000 dequeue.");
    }

    private static ScenarioDefinition BuildQueueMixed(string directory)
    {
        var builder = new ScenarioBuilder();
        for (var i = 0; i < 20_000; i++)
        {
            builder.Push();
        }

        for (var i = 0; i < 10_000; i++)
        {
            builder.Pop();
        }

        for (var i = 0; i < 5_000; i++)
        {
            builder.Peek();
            builder.IsEmpty();
        }

        return builder.Finish("queue-mixed", "Queue", "queue_mixed.txt", directory,
            "40 000 операций: 20 000 enqueue, 10 000 dequeue, 5000 peek, 5000 isEmpty.");
    }

    private sealed class ScenarioBuilder
    {
        private readonly StringBuilder _text = new();
        private int _depth;
        private int _count;
        private int _valueIndex;

        public void Push()
        {
            _depth++;
            Append("1," + NextValue());
        }

        public void Pop()
        {
            if (_depth <= 0)
            {
                throw new InvalidOperationException("Сценарий пытается удалить элемент из пустой структуры.");
            }

            _depth--;
            Append("2");
        }

        public void Peek()
        {
            if (_depth <= 0)
            {
                throw new InvalidOperationException("Сценарий пытается просмотреть пустую структуру.");
            }

            Append("3");
        }

        public void IsEmpty() => Append("4");

        public void Print() => Append("5");

        public ScenarioDefinition Finish(
            string name,
            string family,
            string fileName,
            string directory,
            string description)
        {
            _text.AppendLine();
            var content = _text.ToString();
            var parsed = CommandParser.Parse(content, fileName);
            if (parsed.Issues.Count > 0)
            {
                var issue = parsed.Issues[0];
                throw new InvalidOperationException(
                    $"Сгенерированный сценарий {name} содержит ошибку: {issue.Message}");
            }

            var path = Path.Combine(directory, fileName);
            File.WriteAllText(path, content);

            return new ScenarioDefinition(
                name,
                family,
                fileName,
                path,
                parsed.Commands.Count,
                CommandComposition.Describe(parsed.Commands),
                description);
        }

        private void Append(string token)
        {
            if (_count > 0)
            {
                if (_count % 20 == 0)
                {
                    _text.AppendLine();
                }
                else
                {
                    _text.Append(' ');
                }
            }

            _text.Append(token);
            _count++;
        }

        private string NextValue()
        {
            var index = _valueIndex++;
            return (index % 7) switch
            {
                0 => "cat",
                1 => "hello",
                2 => "56",
                3 => "word",
                4 => "42",
                5 => "orange",
                _ => (index % 1000).ToString(CultureInfo.InvariantCulture)
            };
        }
    }
}
