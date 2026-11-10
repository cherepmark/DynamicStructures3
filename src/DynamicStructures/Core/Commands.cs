using System.Globalization;

namespace DynamicStructures.Core;

public enum CommandKind
{
    Push = 1,
    Pop = 2,
    Peek = 3,
    IsEmpty = 4,
    Print = 5
}

public sealed record Command(int Number, CommandKind Kind, string Raw, string? Value);

public sealed record CommandParseIssue(string Token, int Line, string Message);

public sealed class CommandFile
{
    public CommandFile(string source, string text, IReadOnlyList<Command> commands, IReadOnlyList<CommandParseIssue> issues)
    {
        Source = source;
        Text = text;
        Commands = commands;
        Issues = issues;
    }

    public string Source { get; }

    public string Text { get; }

    public IReadOnlyList<Command> Commands { get; }

    public IReadOnlyList<CommandParseIssue> Issues { get; }

    public bool HasIssues => Issues.Count > 0;
}

public static class CommandParser
{
    public static CommandFile Parse(string text, string source = "<string>")
    {
        ArgumentNullException.ThrowIfNull(text);

        var commands = new List<Command>();
        var issues = new List<CommandParseIssue>();
        var lineNumber = 0;

        using var reader = new StringReader(text);
        string? line;
        while ((line = reader.ReadLine()) is not null)
        {
            lineNumber++;
            var tokens = line.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
            foreach (var token in tokens)
            {
                ParseToken(token, lineNumber, commands, issues);
            }
        }

        return new CommandFile(source, text, commands, issues);
    }

    public static CommandFile Load(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        var fullPath = Path.GetFullPath(path);
        var text = File.ReadAllText(fullPath);
        return Parse(text, fullPath);
    }

    private static void ParseToken(
        string token,
        int lineNumber,
        List<Command> commands,
        List<CommandParseIssue> issues)
    {
        switch (token)
        {
            case "2":
                commands.Add(new Command(commands.Count + 1, CommandKind.Pop, token, null));
                return;
            case "3":
                commands.Add(new Command(commands.Count + 1, CommandKind.Peek, token, null));
                return;
            case "4":
                commands.Add(new Command(commands.Count + 1, CommandKind.IsEmpty, token, null));
                return;
            case "5":
                commands.Add(new Command(commands.Count + 1, CommandKind.Print, token, null));
                return;
            case "1":
                issues.Add(new CommandParseIssue(token, lineNumber, "Команда 1 требует значение в формате 1,value."));
                return;
        }

        if (token.StartsWith("1,", StringComparison.Ordinal))
        {
            var value = token[2..];
            if (value.Length == 0)
            {
                issues.Add(new CommandParseIssue(token, lineNumber, "После запятой в команде 1,value отсутствует значение."));
                return;
            }

            commands.Add(new Command(commands.Count + 1, CommandKind.Push, token, value));
            return;
        }

        issues.Add(new CommandParseIssue(
            token,
            lineNumber,
            "Неизвестная команда. Допустимы: 1,value, 2, 3, 4, 5."));
    }
}

public static class CommandComposition
{
    public static string Describe(IEnumerable<Command> commands)
    {
        var counts = new Dictionary<CommandKind, int>
        {
            [CommandKind.Push] = 0,
            [CommandKind.Pop] = 0,
            [CommandKind.Peek] = 0,
            [CommandKind.IsEmpty] = 0,
            [CommandKind.Print] = 0
        };

        var total = 0;
        foreach (var command in commands)
        {
            counts[command.Kind]++;
            total++;
        }

        return string.Create(
            CultureInfo.InvariantCulture,
            $"push/enqueue={counts[CommandKind.Push]}, pop/dequeue={counts[CommandKind.Pop]}, peek/top={counts[CommandKind.Peek]}, isEmpty={counts[CommandKind.IsEmpty]}, print={counts[CommandKind.Print]}, total={total}");
    }
}
