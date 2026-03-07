using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using Flutter.Rendering;

namespace Flutter;

public class FlutterHost : Control
{
    private readonly RenderView _root = new();
    private readonly PipelineOwner _pipeline;

    public FlutterHost()
    {
        _pipeline = new PipelineOwner(_root);
        _pipeline.Attach(_root);

        ClipToBounds = true;

        var timer = new DispatcherTimer(
            interval: TimeSpan.FromMilliseconds(16),
            DispatcherPriority.Render,
            (_, _) => InvalidateArrange());

        timer.Start();
    }

    internal RenderBox? RootChild => _root.Child;

    public void SetRootChild(RenderBox? child)
    {
        _root.Child = child;
        _pipeline.RequestLayout();
        _pipeline.RequestPaint();
        InvalidateVisual();
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        _pipeline.RequestLayout();
        return base.ArrangeOverride(finalSize);
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);

        var result = new BoxHitTestResult();
        var local = e.GetPosition(this);

        if (!_root.HitTest(result, local))
        {
            return;
        }

        foreach (var entry in result.Path)
        {
            entry.Target.HandleEvent(e, entry);
            if (e.Handled)
            {
                break;
            }
        }
    }

    public override void Render(DrawingContext context)
    {
        _pipeline.FlushLayout(Bounds.Size);
        _pipeline.FlushPaint(new PaintingContext(context));
    }
}
