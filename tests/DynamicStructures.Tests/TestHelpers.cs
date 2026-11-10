using DynamicStructures.DataStructures;

namespace DynamicStructures.Tests;

internal static class TestHelpers
{
    public static SinglyLinkedList<T> List<T>(params T[] values)
    {
        var list = new SinglyLinkedList<T>();
        foreach (var value in values)
        {
            list.AddLast(value);
        }

        return list;
    }

    public static string TempDirectory()
    {
        var directory = Path.Combine(
            Path.GetTempPath(),
            "DynamicStructures.Tests",
            Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        return directory;
    }

    public static void DeleteDirectory(string directory)
    {
        try
        {
            if (Directory.Exists(directory))
            {
                Directory.Delete(directory, recursive: true);
            }
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
    }
}
