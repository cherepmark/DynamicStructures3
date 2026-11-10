using System.Diagnostics.CodeAnalysis;
using DynamicStructures.DataStructures;

namespace DynamicStructures.Applications;

public sealed record PrintJob(string Document, int Pages);

/// <summary>
/// Очередь печати: задания обрабатываются строго в порядке поступления (FIFO).
/// По умолчанию используется собственная ListBasedQueue, но можно передать
/// любую реализацию IQueue.
/// </summary>
public sealed class PrintQueueSimulation
{
    private readonly IQueue<PrintJob> _queue;
    private readonly List<PrintJob> _processed = new();

    public PrintQueueSimulation(IQueue<PrintJob>? queue = null)
    {
        _queue = queue ?? new ListBasedQueue<PrintJob>();
    }

    public string ImplementationName => _queue.ImplementationName;

    public int PendingCount => _queue.Count;

    public bool IsEmpty => _queue.IsEmpty;

    public IReadOnlyList<PrintJob> Processed => _processed;

    public void Submit(PrintJob job)
    {
        ArgumentNullException.ThrowIfNull(job);
        if (string.IsNullOrWhiteSpace(job.Document))
        {
            throw new ArgumentException("Имя документа не может быть пустым.", nameof(job));
        }

        if (job.Pages <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(job), "Количество страниц должно быть положительным.");
        }

        _queue.Enqueue(job);
    }

    public bool TryProcessNext([MaybeNullWhen(false)] out PrintJob job)
    {
        if (!_queue.TryDequeue(out job))
        {
            return false;
        }

        _processed.Add(job);
        return true;
    }

    public IReadOnlyList<PrintJob> ProcessAll()
    {
        while (TryProcessNext(out _))
        {
        }

        return _processed;
    }

    public PrintJob[] Pending() => _queue.Snapshot();

    public void WritePending(TextWriter writer)
    {
        ArgumentNullException.ThrowIfNull(writer);
        var pending = _queue.Snapshot();
        if (pending.Length == 0)
        {
            writer.WriteLine("  (очередь печати пуста)");
            return;
        }

        for (var i = 0; i < pending.Length; i++)
        {
            writer.WriteLine($"  {i + 1}. {pending[i].Document}: {pending[i].Pages} стр.");
        }
    }
}
