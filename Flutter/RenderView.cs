using Avalonia;
using Avalonia.Media;
using Flutter.Rendering;

namespace Flutter;

public sealed class RenderView : RenderBox
{
    public RenderBox? Child { get; set; }

    public override void Layout(BoxConstraints constraints, bool parentUsesSize = false)
    {
        // RenderView просто прокидывает constraints ребёнку
        if (Child != null)
        {
            Child.Layout(constraints);
            Size = Child.Size;
        }
        else
        {
            Size = constraints.Constrain(new Size());
        }
    }

    public override void Paint(DrawingContext ctx, Point offset)
    {
        Child?.Paint(ctx, offset);
    }
}