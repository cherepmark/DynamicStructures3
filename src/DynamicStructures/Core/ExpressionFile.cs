namespace DynamicStructures.Core;

public sealed record ExpressionLine(int LineNumber, string Expression);

public static class ExpressionFile
{
    public static IReadOnlyList<ExpressionLine> Load(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        var fullPath = Path.GetFullPath(path);
        var result = new List<ExpressionLine>();
        var lineNumber = 0;

        foreach (var line in File.ReadLines(fullPath))
        {
            lineNumber++;
            var trimmed = line.Trim();
            if (trimmed.Length == 0 || trimmed.StartsWith('#'))
            {
                continue;
            }

            result.Add(new ExpressionLine(lineNumber, trimmed));
        }

        return result;
    }
}
