using Avalonia;
using Flutter.Rendering;
using Flutter.UI;

namespace Flutter.Gestures;

public sealed class GestureBinding
{
    public static GestureBinding Instance { get; } = new();

    private readonly Dictionary<int, HitTestResult> _hitTests = [];
    private readonly Dictionary<int, Point> _lastPositions = [];

    public PointerRouter PointerRouter { get; } = new();

    public GestureArenaManager GestureArena { get; } = new();

    public void HandlePointerEvent(RenderView root, PointerEvent @event)
    {
        var eventWithDelta = AttachDelta(@event);
        HitTestResult? hitTestResult = null;

        switch (@event)
        {
            case PointerDownEvent:
            {
                var result = new BoxHitTestResult();
                root.HitTest(result, @event.Position);
                _hitTests[@event.Pointer] = result;
                hitTestResult = result;
                break;
            }
            case PointerMoveEvent or PointerUpEvent or PointerCancelEvent:
            {
                _hitTests.TryGetValue(@event.Pointer, out hitTestResult);
                break;
            }
            case PointerHoverEvent:
            {
                var result = new BoxHitTestResult();
                root.HitTest(result, @event.Position);
                hitTestResult = result;
                break;
            }
        }

        DispatchEvent(eventWithDelta, hitTestResult);

        if (@event is PointerDownEvent)
        {
            GestureArena.Close(@event.Pointer);
        }

        if (@event is PointerUpEvent or PointerCancelEvent)
        {
            GestureArena.Sweep(@event.Pointer);
            _hitTests.Remove(@event.Pointer);
            _lastPositions.Remove(@event.Pointer);
        }
    }

    public void DispatchEvent(PointerEvent @event, HitTestResult? hitTestResult)
    {
        if (hitTestResult != null)
        {
            foreach (var entry in hitTestResult.Path)
            {
                var transformedEvent = entry.TransformEvent(@event);
                entry.Target.HandleEvent(transformedEvent, entry);
            }
        }

        PointerRouter.Route(@event);
    }

    internal void ResetForTests()
    {
        _hitTests.Clear();
        _lastPositions.Clear();
        PointerRouter.Reset();
        GestureArena.Reset();
    }

    private PointerEvent AttachDelta(PointerEvent @event)
    {
        var pointer = @event.Pointer;
        if (!_lastPositions.TryGetValue(pointer, out var previousPosition))
        {
            _lastPositions[pointer] = @event.Position;
            return @event.WithDelta(default);
        }

        var delta = @event.Position - previousPosition;
        _lastPositions[pointer] = @event.Position;
        return @event.WithDelta(delta);
    }
}
