using Avalonia;
using Flutter.UI;

namespace Flutter.Rendering;

public enum HitTestBehavior
{
    DeferToChild,
    Opaque,
    Translucent
}

public class HitTestEntry(RenderObject target)
{
    public RenderObject Target { get; } = target;

    public virtual PointerEvent TransformEvent(PointerEvent @event)
    {
        return @event;
    }
}

public sealed class BoxHitTestEntry(RenderBox target, Point localPosition) : HitTestEntry(target)
{
    public Point LocalPosition { get; } = localPosition;

    public override PointerEvent TransformEvent(PointerEvent @event)
    {
        return @event.WithLocalCoordinates(LocalPosition, @event.Delta);
    }
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
