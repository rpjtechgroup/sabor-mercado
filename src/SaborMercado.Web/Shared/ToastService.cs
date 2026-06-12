namespace SaborMercado.Web.Shared;

public sealed class ToastService : IDisposable
{
    public const int MaxVisible = 3;
    public static readonly TimeSpan DefaultAutoDismiss = TimeSpan.FromSeconds(6);

    private readonly TimeProvider _clock;
    private readonly TimeSpan _autoDismiss;
    private readonly List<ToastEntry> _items = [];
    private readonly Dictionary<Guid, IDisposable> _timers = [];
    private bool _disposed;

    public ToastService(TimeProvider clock)
        : this(clock, DefaultAutoDismiss)
    {
    }

    public ToastService(TimeProvider clock, TimeSpan autoDismiss)
    {
        _clock = clock;
        _autoDismiss = autoDismiss;
    }

    public event Action? StateChanged;

    public IReadOnlyList<ToastEntry> Items => _items;

    public Guid Show(string text, ToastSeverity severity, Action? onDismiss = null)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        while (_items.Count >= MaxVisible)
        {
            Dismiss(_items[0].Id);
        }

        var id = Guid.NewGuid();
        _items.Add(new ToastEntry(id, text, severity, onDismiss));

        var timer = _clock.CreateTimer(_ => Dismiss(id), null, _autoDismiss, Timeout.InfiniteTimeSpan);
        _timers[id] = timer;

        StateChanged?.Invoke();
        return id;
    }

    public void Dismiss(Guid id)
    {
        if (_disposed)
        {
            return;
        }

        var index = _items.FindIndex(t => t.Id == id);
        if (index < 0)
        {
            return;
        }

        var entry = _items[index];
        _items.RemoveAt(index);

        if (_timers.Remove(id, out var timer))
        {
            timer.Dispose();
        }

        entry.OnDismiss?.Invoke();
        StateChanged?.Invoke();
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        foreach (var timer in _timers.Values)
        {
            timer.Dispose();
        }

        _timers.Clear();
        _items.Clear();
    }
}
