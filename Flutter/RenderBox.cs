using Avalonia;
using Avalonia.Media;

namespace Flutter;

public abstract class RenderBox : RenderObject
{
    public Size Size { get; protected set; }

    public override void Paint(DrawingContext ctx, Point offset)
    {
        /* default: nothing */
    }
}