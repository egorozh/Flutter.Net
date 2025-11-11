using Avalonia;
using Avalonia.Media;
using Flutter.Rendering;

namespace Flutter;

static class MathEx
{
    public static bool Near(double a, double b, double eps = 0.005) => Math.Abs(a - b) <= eps;
}

public sealed class RenderAnimatedBox : RenderBox, IDisposable
{
    private readonly AnimationController _controller;

    // Интерполируем ширину, высоту и цвет
    private readonly DoubleTween _double = new();
    private readonly ColorTween _color = new();

    private readonly double _fromW, _toW;
    private readonly double _fromH, _toH;
    private readonly Color _fromC, _toC;

    public SolidColorBrush Brush { get; } = new SolidColorBrush(Colors.White);

    public RenderAnimatedBox(
        AnimationController controller,
        double fromWidth, double toWidth,
        double fromHeight, double toHeight,
        Color fromColor, Color toColor)
    {
        _controller = controller;
        _fromW = fromWidth;
        _toW = toWidth;
        _fromH = fromHeight;
        _toH = toHeight;
        _fromC = fromColor;
        _toC = toColor;

        _controller.Changed += OnTick;
    }

    private double _currentW, _currentH;

    private void OnTick()
    {
        double t = _controller.Evaluate();

        double nextW = _double.Evaluate(t, _fromW, _toW);
        double nextH = _double.Evaluate(t, _fromH, _toH);
        var nextC = _color.Evaluate(t, _fromC, _toC);

        bool affectsLayout = !MathEx.Near(nextW, _currentW) || !MathEx.Near(nextH, _currentH);

        _currentW = nextW;
        _currentH = nextH;
        Brush.Color = nextC;

        if (affectsLayout) MarkNeedsLayout();
        else MarkNeedsPaint();
    }

    public override void Layout(BoxConstraints constraints)
    {
        // Задаёмся анимируемыми размерами, но не превышаем constraints
        var desired = new Size(_currentW <= 0 ? _fromW : _currentW,
            _currentH <= 0 ? _fromH : _currentH);
        Size = constraints.Constrain(desired);
    }

    public override void Paint(DrawingContext ctx, Point offset)
    {
        var rect = new Rect(offset, Size);
        ctx.DrawRectangle(Brush, null, rect, 16, 16);
    }

    public void Dispose()
    {
        _controller.Changed -= OnTick;
    }
}