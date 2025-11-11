using Avalonia;
using Avalonia.Media;
using Flutter.Rendering;

namespace Flutter;

public sealed class RenderColoredBox : RenderBox
{
    public SolidColorBrush Brush { get; }
    public double? Width { get; set; }
    public double? Height { get; set; }
    public double Radius { get; set; }

    public RenderColoredBox(Color color, double? width = null, double? height = null, double radius = 12)
    {
        Brush = new SolidColorBrush(color);
        Width = width;
        Height = height;
        Radius = radius;
    }

    public override void Layout(BoxConstraints constraints)
    {
        double w = Width ?? constraints.MaxWidth;
        double h = Height ?? 80;
        Size = constraints.Constrain(new Size(w, h));
    }

    public override void Paint(DrawingContext ctx, Point offset)
    {
        var rect = new Rect(offset, Size);
        ctx.DrawRectangle(Brush, pen: null, rect: rect, Radius, Radius);
    }
}