using Avalonia;
using Avalonia.Media;

namespace Flutter;

public sealed class PipelineOwner
{
    public RenderView Root { get; }

    private bool _needsLayout;
    private bool _needsPaint;

    public PipelineOwner(RenderView root) => Root = root;

    public void Attach(RenderObject obj)
    {
        obj.Owner = this;
        
        obj.VisitChildren(Attach);
    }

    public void RequestLayout() => _needsLayout = true;
    
    public void RequestPaint() => _needsPaint = true;

    public void FlushLayout(Size rootSize)
    {
        if (!_needsLayout) return;
        
        _needsLayout = false;
        
        var constraints = new BoxConstraints(0, rootSize.Width, 0, rootSize.Height);
        
        Root.Layout(constraints);
        
        _needsPaint = true; // layout обычно ведёт к перерисовке
    }

    public void FlushPaint(DrawingContext ctx)
    {
        if (!_needsPaint) return;
        
        _needsPaint = false;
        
        Root.Paint(ctx, new Point(0, 0));
    }
}