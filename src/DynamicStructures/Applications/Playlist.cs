using DynamicStructures.DataStructures;

namespace DynamicStructures.Applications;

public sealed record Song(string Title, string Artist, int DurationSeconds);

/// <summary>
/// Плейлист на собственном связном списке: добавление, удаление и перестановка
/// песен выполняются над узлами списка.
/// </summary>
public sealed class Playlist
{
    private readonly SinglyLinkedList<Song> _songs = new();

    public Playlist(string name = "Плейлист")
    {
        Name = name;
    }

    public string Name { get; }

    public int Count => _songs.Count;

    public void Add(Song song)
    {
        ArgumentNullException.ThrowIfNull(song);
        if (string.IsNullOrWhiteSpace(song.Title))
        {
            throw new ArgumentException("Название песни не может быть пустым.", nameof(song));
        }

        if (song.DurationSeconds < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(song), "Длительность не может быть отрицательной.");
        }

        _songs.AddLast(song);
    }

    public bool Remove(string title)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        var index = IndexOf(title);
        if (index < 0)
        {
            return false;
        }

        _songs.RemoveAt(index, out _);
        return true;
    }

    public int IndexOf(string title)
    {
        var index = 0;
        foreach (var song in _songs.Items())
        {
            if (string.Equals(song.Title, title, StringComparison.Ordinal))
            {
                return index;
            }

            index++;
        }

        return -1;
    }

    /// <summary>Перемещает песню так, чтобы в результате она оказалась по индексу targetIndex.</summary>
    public bool Move(int sourceIndex, int targetIndex)
    {
        if (sourceIndex < 0 || sourceIndex >= _songs.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(sourceIndex));
        }

        if (targetIndex < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(targetIndex));
        }

        if (sourceIndex == targetIndex)
        {
            return true;
        }

        var song = _songs.NodeAt(sourceIndex).Value;
        _songs.RemoveAt(sourceIndex, out _);
        var destination = Math.Min(targetIndex, _songs.Count);
        _songs.InsertAt(destination, song);
        return true;
    }

    public Song this[int index] => _songs.NodeAt(index).Value;

    public Song[] ToArray() => _songs.ToArray();

    public string[] Titles() => _songs.ToArray().Select(song => song.Title).ToArray();

    public TimeSpan TotalDuration =>
        TimeSpan.FromSeconds(_songs.ToArray().Sum(song => song.DurationSeconds));

    public void WriteTo(TextWriter writer)
    {
        ArgumentNullException.ThrowIfNull(writer);
        if (_songs.Count == 0)
        {
            writer.WriteLine("  (плейлист пуст)");
            return;
        }

        var index = 1;
        foreach (var song in _songs.Items())
        {
            writer.WriteLine(
                $"  {index++}. {song.Title} — {song.Artist} ({song.DurationSeconds} с)");
        }
    }
}
