using Avalonia;
using Avalonia.Media;

namespace Flutter;

public sealed class RenderColumn : RenderBox
{
    public double Spacing { get; set; } = 8;
    public List<RenderBox> Children { get; } = new();
    private readonly List<Point> _childOffsets = new();

    public override void Layout(BoxConstraints constraints)
    {
        _childOffsets.Clear();
        double y = 0;
        double maxWidth = 0;

        foreach (var child in Children)
        {
            child.Parent = this;

            child.Layout(new BoxConstraints(0, constraints.MaxWidth, 0, double.PositiveInfinity));

            _childOffsets.Add(new Point(0, y));

            y += child.Size.Height + Spacing;

            maxWidth = Math.Max(maxWidth, child.Size.Width);
        }

        if (Children.Count > 0)
            y -= Spacing; // убрать последний зазор

        Size = constraints.Constrain(new Size(maxWidth, y));
    }

    public override void Paint(DrawingContext ctx, Point offset)
    {
        for (int i = 0; i < Children.Count; i++)
        {
            var child = Children[i];
            var childOffset = _childOffsets[i];
            child.Paint(ctx, offset + childOffset);
        }
    }
}