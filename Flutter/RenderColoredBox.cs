using Avalonia;
using Avalonia.Media;
using Flutter.Rendering;

namespace Flutter;

public sealed class RenderColoredBox : RenderBox
{
    public SolidColorBrush Brush { get; }

    public double Radius { get; set; }

    public RenderColoredBox(Color color, double radius = 12)
    {
        Brush = new SolidColorBrush(color);
  
        Radius = radius;
    }

    protected override void PerformLayout()
    {
        Size = new Size(40,40);
    }

    public override void Paint(PaintingContext ctx, Point offset)
    {
        var rect = new Rect(offset, Size);
        
        ctx.Context.DrawRectangle(Brush, pen: null, rect: rect, Radius, Radius);
    }
}