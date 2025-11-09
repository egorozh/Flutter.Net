using System.Diagnostics;
using Avalonia.Threading;

namespace Flutter;

public static class Scheduler
{
    private static readonly List<Ticker> _active = new();
    private static DispatcherTimer? _timer;
    private static bool _running;
    private static readonly Stopwatch _sw = Stopwatch.StartNew();

    public static double CurrentSeconds => _sw.Elapsed.TotalSeconds;

    public static void Add(Ticker t)
    {
        if (!_active.Contains(t))
            _active.Add(t);
        
        EnsureRunning();
    }

    public static void Remove(Ticker t)
    {
        _active.Remove(t);
        
        if (_active.Count == 0)
            Stop();
    }

    private static void EnsureRunning()
    {
        if (_running) return;
        
        _timer = new DispatcherTimer(TimeSpan.FromMilliseconds(16), DispatcherPriority.Render, (_, _) => Tick());
        
        _timer.Start();
        
        _running = true;
    }

    private static void Stop()
    {
        if (!_running) return;
        _timer?.Stop();
        _running = false;
    }

    private static void Tick()
    {
        double now = CurrentSeconds;
        // Копируем список, чтобы безопасно обрабатывать отписки по ходу тика
        var snapshot = _active.ToArray();
        foreach (var t in snapshot)
            if (t.Active)
                t.InternalTick(now);
    }
}