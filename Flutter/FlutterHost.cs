using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using Flutter.Rendering;

namespace Flutter;

public sealed class FlutterHost : Control
{
    private readonly RenderView _root = new();

    private readonly PipelineOwner _pipeline;

    public FlutterHost()
    {
        _pipeline = new PipelineOwner(_root);

        ClipToBounds = true;

        // Simple vsync
        var timer = new DispatcherTimer(
            interval: TimeSpan.FromMilliseconds(16),
            DispatcherPriority.Render,
            (_, _) => InvalidateArrange());

        timer.Start();
    }

    public void SetRootChild(RenderBox child)
    {
        _root.Child = child;
        _pipeline.RequestLayout();

        InvalidateVisual();
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        _pipeline.RequestLayout();

        return base.ArrangeOverride(finalSize);
    }

    public override void Render(DrawingContext context)
    {
        _pipeline.FlushLayout(Bounds.Size);

        _pipeline.FlushPaint(context);
    }
}