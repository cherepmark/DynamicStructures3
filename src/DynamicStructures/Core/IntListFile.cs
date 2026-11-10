using System.Globalization;
using DynamicStructures.DataStructures;

namespace DynamicStructures.Core;

/// <summary>Чтение списков целых чисел из файла или строки.</summary>
public static class IntListFile
{
    public static SinglyLinkedList<int> Load(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        var fullPath = Path.GetFullPath(path);
        return Parse(File.ReadAllText(fullPath), fullPath);
    }

    public static SinglyLinkedList<int> Parse(string text, string source = "<string>")
    {
        ArgumentNullException.ThrowIfNull(text);
        var result = new SinglyLinkedList<int>();
        var lineNumber = 0;

        using var reader = new StringReader(text);
        string? line;
        while ((line = reader.ReadLine()) is not null)
        {
            lineNumber++;
            var commentIndex = line.IndexOf('#');
            if (commentIndex >= 0)
            {
                line = line[..commentIndex];
            }

            var tokens = line.Split(
                [' ', '\t', '\r', ',', ';'],
                StringSplitOptions.RemoveEmptyEntries);
            foreach (var token in tokens)
            {
                if (!int.TryParse(token, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value))
                {
                    throw new FormatException(
                        $"{source}: строка {lineNumber}: '{token}' не является целым числом.");
                }

                result.AddLast(value);
            }
        }

        return result;
    }
}
