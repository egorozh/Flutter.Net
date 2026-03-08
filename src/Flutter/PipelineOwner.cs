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
    private readonly HashSet<RenderObject> _nodesNeedingLayout = [];
    private readonly HashSet<RenderObject> _nodesNeedingPaint = [];
    private readonly HashSet<RenderObject> _nodesNeedingSemantics = [];
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
        RequestLayoutFor(Root);
    }

    internal void RequestLayoutFor(RenderObject node)
    {
        if (!_nodesNeedingLayout.Add(node))
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
        RequestPaintFor(Root);
    }

    internal void RequestPaintFor(RenderObject node)
    {
        if (!_nodesNeedingPaint.Add(node))
        {
            return;
        }

        _needsPaint = true;
        OnNeedVisualUpdate?.Invoke();
    }

    public void RequestSemanticsUpdate()
    {
        RequestSemanticsUpdateFor(Root);
    }

    internal void RequestSemanticsUpdateFor(RenderObject node)
    {
        if (!_nodesNeedingSemantics.Add(node))
        {
            return;
        }

        _needsSemantics = true;
        OnNeedVisualUpdate?.Invoke();
    }

    public void FlushLayout(Size rootSize)
    {
        if (!_needsLayout) return;

        var constraints = new BoxConstraints(0, rootSize.Width, 0, rootSize.Height);
        
        while (_nodesNeedingLayout.Count > 0)
        {
            var dirtyNodes = _nodesNeedingLayout
                .OrderBy(static node => node.Depth)
                .ToArray();

            _nodesNeedingLayout.Clear();
            _needsLayout = false;

            foreach (var node in dirtyNodes)
            {
                if (!node.Attached || !ReferenceEquals(node.Owner, this))
                {
                    continue;
                }

                if (ReferenceEquals(node, Root))
                {
                    Root.Layout(constraints);
                    continue;
                }

                if (!node.NeedsLayoutOrDescendantNeedsLayout)
                {
                    continue;
                }

                if (!node.HasBoxConstraints)
                {
                    RequestLayout();
                    continue;
                }

                node.Layout(node.CurrentBoxConstraints);
            }
        }

        _needsLayout = false;
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
        if (!_needsPaint)
        {
            return;
        }

        while (_nodesNeedingPaint.Count > 0)
        {
            var dirtyNodes = _nodesNeedingPaint
                .OrderByDescending(static node => node.Depth)
                .ToArray();

            _nodesNeedingPaint.Clear();
            _needsPaint = false;

            foreach (var node in dirtyNodes)
            {
                if (!node.Attached || !ReferenceEquals(node.Owner, this))
                {
                    continue;
                }

                if (!node.NeedsPaint && !ReferenceEquals(node, Root))
                {
                    continue;
                }

                if (ReferenceEquals(node, Root))
                {
                    RepaintRoot();
                    continue;
                }

                if (!node.IsRepaintBoundary || node._layer is not OffsetLayer layer || layer.Parent == null)
                {
                    RequestPaintFor(Root);
                    continue;
                }

                layer.RemoveAllChildren();
                node._paintWithContext(new PaintingContext(layer), new Point(0, 0));
            }
        }

        _needsPaint = false;
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

        while (_nodesNeedingSemantics.Count > 0)
        {
            var hasOwnedDirtyNodes = _nodesNeedingSemantics.Any(node =>
                node.Attached && ReferenceEquals(node.Owner, this));

            _nodesNeedingSemantics.Clear();
            _needsSemantics = false;

            if (!hasOwnedDirtyNodes)
            {
                continue;
            }

            var roots = new List<SemanticsNode>();
            Root.BuildSemantics(_semanticsOwner, new Point(0, 0), roots);
            _semanticsOwner.UpdateRoot(roots);
        }

        _needsSemantics = false;
    }

    private void RepaintRoot()
    {
        _rootLayer.RemoveAllChildren();
        Root._paintWithContext(new PaintingContext(_rootLayer), new Point(0, 0));
    }
}
