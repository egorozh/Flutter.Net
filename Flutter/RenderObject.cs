using Avalonia;
using Avalonia.Media;

namespace Flutter;

public abstract class RenderObject
{
    public RenderObject? Parent { get; internal set; }
        
    internal PipelineOwner? Owner { get; set; }

    public abstract void Layout(BoxConstraints constraints);
    
    public abstract void Paint(DrawingContext ctx, Point offset);

    public virtual void VisitChildren(Action<RenderObject> visitor)
    {
    }

    protected void MarkNeedsLayout() => Owner?.RequestLayout();

    protected void MarkNeedsPaint() => Owner?.RequestPaint();
}