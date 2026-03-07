using Avalonia;

namespace Flutter.Rendering;

public class HitTestEntry(RenderObject target)
{
    public RenderObject Target { get; } = target;
}

public class HitTestResult
{
    private readonly List<HitTestEntry> _path = [];

    public IReadOnlyList<HitTestEntry> Path => _path;

    public void Add(HitTestEntry entry)
    {
        _path.Add(entry);
    }
}

public sealed class BoxHitTestResult : HitTestResult
{
}
