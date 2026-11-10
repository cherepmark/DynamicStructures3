using System.Diagnostics.CodeAnalysis;

namespace DynamicStructures.DataStructures;

/// <summary>Общий интерфейс двух реализаций очереди.</summary>
public interface IQueue<T>
{
    string ImplementationName { get; }

    int Count { get; }

    bool IsEmpty { get; }

    void Enqueue(T item);

    bool TryDequeue([MaybeNullWhen(false)] out T item);

    bool TryPeek([MaybeNullWhen(false)] out T item);

    /// <summary>Снимок содержимого от начала очереди к концу.</summary>
    T[] Snapshot();

    void WriteTo(TextWriter writer, string separator = " ");
}
