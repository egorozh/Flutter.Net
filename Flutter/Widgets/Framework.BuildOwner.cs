using Avalonia.Threading;

namespace Flutter.Widgets;

/// <summary>
/// BuildOwner + Scheduler
/// </summary>
public sealed class BuildOwner
{
    private readonly SortedSet<Element> _dirty = new(ElementDepthComparer.Instance);
    private readonly HashSet<Element> _tracked = new();
    private bool _scheduled;

    public void RegisterElement(Element e) => _tracked.Add(e);
    public void UnregisterElement(Element e) => _tracked.Remove(e);

    public void ScheduleBuild(Element e)
    {
        _dirty.Add(e);

        if (_scheduled) return;

        _scheduled = true;

        Dispatcher.UIThread.Post(PerformBuild, DispatcherPriority.Render);
    }

    public void MarkSubtreeNeedsBuild(Element root)
    {
        foreach (var e in _tracked.Where(x => IsDescendantOf(x, root)))
            ScheduleBuild(e);
    }

    private static bool IsDescendantOf(Element node, Element root)
    {
        for (var p = node.Parent; p != null; p = p.Parent)
            if (ReferenceEquals(p, root))
                return true;
        return false;
    }

    private void PerformBuild()
    {
        _scheduled = false;
        
        while (_dirty.Count > 0)
        {
            var e = _dirty.Max!; // deepest first
            _dirty.Remove(e);
            if (e.Owner != this) continue;
            e.Rebuild();
        }
    }

    private sealed class ElementDepthComparer : IComparer<Element>
    {
        public static readonly ElementDepthComparer Instance = new();

        public int Compare(Element? x, Element? y)
        {
            if (ReferenceEquals(x, y)) return 0;
            if (x is null) return -1;
            if (y is null) return 1;
            int d = x.Depth.CompareTo(y.Depth);
            if (d != 0) return d;
            // fallback to hash for stability
            return x.GetHashCode().CompareTo(y.GetHashCode());
        }
    }
}