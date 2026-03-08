using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Flutter.Rendering;

namespace Flutter;

public class FlutterHost : Control
{
    private readonly RenderView _root = new();
    private readonly PipelineOwner _pipeline;
    private bool _isSubscribedToScheduler;

    public FlutterHost()
    {
        _pipeline = new PipelineOwner(_root);
        _pipeline.OnNeedVisualUpdate = ScheduleVisualUpdate;
        _pipeline.Attach(_root);

        ClipToBounds = true;
        EnsureSchedulerSubscription();
    }

    internal RenderBox? RootChild => _root.Child;

    public void SetRootChild(RenderBox? child)
    {
        _root.Child = child;
        _pipeline.RequestLayout();
        _pipeline.RequestPaint();
        ScheduleVisualUpdate();
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
        _pipeline.FlushCompositingBits();
        _pipeline.FlushPaint();
        _pipeline.CompositeFrame(context);
        _pipeline.FlushSemantics();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        EnsureSchedulerSubscription();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        RemoveSchedulerSubscription();
        base.OnDetachedFromVisualTree(e);
    }

    protected virtual void OnDrawFrame(TimeSpan timestamp)
    {
    }

    protected void ScheduleVisualUpdate()
    {
        Scheduler.ScheduleFrame();
    }

    private void HandleSchedulerDrawFrame(TimeSpan timestamp)
    {
        if (!IsVisible)
        {
            return;
        }

        OnDrawFrame(timestamp);
        _pipeline.FlushLayout(Bounds.Size);
        _pipeline.FlushCompositingBits();

        if (_pipeline.NeedsPaint)
        {
            InvalidateVisual();
        }
        else
        {
            _pipeline.FlushSemantics();
        }
    }

    private void EnsureSchedulerSubscription()
    {
        if (_isSubscribedToScheduler)
        {
            return;
        }

        Scheduler.AddPersistentFrameCallback(HandleSchedulerDrawFrame);
        _isSubscribedToScheduler = true;
    }

    private void RemoveSchedulerSubscription()
    {
        if (!_isSubscribedToScheduler)
        {
            return;
        }

        Scheduler.RemovePersistentFrameCallback(HandleSchedulerDrawFrame);
        _isSubscribedToScheduler = false;
    }
}
