using Avalonia;
using Avalonia.Media;
using Flutter.Rendering;

namespace Flutter;

public sealed class PipelineOwner
{
    public RenderView Root { get; }
    public Action? OnNeedVisualUpdate { get; set; }
    internal SemanticsOwner SemanticsOwner => _semanticsOwner;
    internal OffsetLayer RootLayer => _rootLayer;

    private bool _needsLayout;
    private bool _needsCompositingBitsUpdate;
    private bool _needsPaint;
    private bool _needsSemantics;
    private readonly SemanticsOwner _semanticsOwner = new();
    private readonly OffsetLayer _rootLayer = new();

    internal bool NeedsPaint => _needsPaint;

    public PipelineOwner(RenderView root) => Root = root;

    public void Attach(RenderObject obj)
    {
        obj.Attach(this);
        
        obj.VisitChildren(Attach);
    }

    public void RequestLayout()
    {
        if (_needsLayout)
        {
            return;
        }

        _needsLayout = true;
        OnNeedVisualUpdate?.Invoke();
    }

    public void RequestCompositingBitsUpdate()
    {
        if (_needsCompositingBitsUpdate)
        {
            return;
        }

        _needsCompositingBitsUpdate = true;
        OnNeedVisualUpdate?.Invoke();
    }

    public void RequestPaint()
    {
        if (_needsPaint)
        {
            return;
        }

        _needsPaint = true;
        OnNeedVisualUpdate?.Invoke();
    }

    public void RequestSemanticsUpdate()
    {
        if (_needsSemantics)
        {
            return;
        }

        _needsSemantics = true;
        OnNeedVisualUpdate?.Invoke();
    }

    public void FlushLayout(Size rootSize)
    {
        if (!_needsLayout) return;

        _needsLayout = false;

        var constraints = new BoxConstraints(0, rootSize.Width, 0, rootSize.Height);

        Root.Layout(constraints);

        // A completed layout pass invalidates downstream pipeline stages.
        _needsCompositingBitsUpdate = true;
        _needsPaint = true;
        _needsSemantics = true;
    }

    public void FlushCompositingBits()
    {
        if (!_needsCompositingBitsUpdate)
        {
            return;
        }

        _needsCompositingBitsUpdate = false;
        Root.FlushCompositingBits();
    }

    public void FlushPaint()
    {
        if (!_needsPaint) return;

        _needsPaint = false;

        _rootLayer.RemoveAllChildren();
        Root.Paint(new PaintingContext(_rootLayer), new Point(0, 0));
    }

    public void CompositeFrame(DrawingContext context)
    {
        _rootLayer.AddToScene(context, new Point(0, 0));
    }

    public void FlushSemantics()
    {
        if (!_needsSemantics)
        {
            return;
        }

        _needsSemantics = false;
        var roots = new List<SemanticsNode>();
        Root.BuildSemantics(_semanticsOwner, new Point(0, 0), roots);
        _semanticsOwner.UpdateRoot(roots);
    }
}
