using Flutter.Foundation;

namespace Flutter.Rendering;

public interface IScrollMetrics
{
    double Pixels { get; }
    double MinScrollExtent { get; }
    double MaxScrollExtent { get; }
    double ViewportDimension { get; }
}

public abstract class ScrollPhysics
{
    protected ScrollPhysics(ScrollPhysics? parent = null)
    {
        Parent = parent;
    }

    public ScrollPhysics? Parent { get; }

    public virtual double ApplyPhysicsToUserOffset(IScrollMetrics position, double offset)
    {
        if (Parent != null)
        {
            return Parent.ApplyPhysicsToUserOffset(position, offset);
        }

        return offset;
    }

    public virtual double ApplyBoundaryConditions(IScrollMetrics position, double value)
    {
        if (Parent != null)
        {
            return Parent.ApplyBoundaryConditions(position, value);
        }

        return 0;
    }
}

public sealed class ClampingScrollPhysics : ScrollPhysics
{
    public ClampingScrollPhysics(ScrollPhysics? parent = null) : base(parent)
    {
    }

    public override double ApplyBoundaryConditions(IScrollMetrics position, double value)
    {
        if (value < position.Pixels && position.Pixels <= position.MinScrollExtent)
        {
            return value - position.Pixels;
        }

        if (position.MaxScrollExtent <= position.Pixels && position.Pixels < value)
        {
            return value - position.Pixels;
        }

        if (value < position.MinScrollExtent && position.MinScrollExtent < position.Pixels)
        {
            return value - position.MinScrollExtent;
        }

        if (position.Pixels < position.MaxScrollExtent && position.MaxScrollExtent < value)
        {
            return value - position.MaxScrollExtent;
        }

        return base.ApplyBoundaryConditions(position, value);
    }
}

public sealed class ScrollPosition : ChangeNotifier, IScrollMetrics
{
    private readonly ScrollPhysics _physics;
    private double _pixels;
    private double _minScrollExtent;
    private double _maxScrollExtent;
    private double _viewportDimension;

    public ScrollPosition(double initialPixels = 0.0, ScrollPhysics? physics = null)
    {
        _pixels = initialPixels;
        _physics = physics ?? new ClampingScrollPhysics();
    }

    public double Pixels => _pixels;

    public double MinScrollExtent => _minScrollExtent;

    public double MaxScrollExtent => _maxScrollExtent;

    public double ViewportDimension => _viewportDimension;

    public ScrollPhysics Physics => _physics;

    public void JumpTo(double value)
    {
        SetPixels(value);
    }

    public void ApplyUserOffset(double delta)
    {
        var adjusted = Physics.ApplyPhysicsToUserOffset(this, delta);
        SetPixels(Pixels - adjusted);
    }

    public void ApplyPointerScrollDelta(double delta)
    {
        SetPixels(Pixels + delta);
    }

    public bool ApplyViewportDimension(double viewportDimension)
    {
        if (Math.Abs(_viewportDimension - viewportDimension) < 0.0001)
        {
            return false;
        }

        _viewportDimension = viewportDimension;
        return true;
    }

    public bool ApplyContentDimensions(double minScrollExtent, double maxScrollExtent)
    {
        var minChanged = Math.Abs(_minScrollExtent - minScrollExtent) > 0.0001;
        var maxChanged = Math.Abs(_maxScrollExtent - maxScrollExtent) > 0.0001;
        _minScrollExtent = minScrollExtent;
        _maxScrollExtent = maxScrollExtent;

        var oldPixels = _pixels;
        _pixels = Math.Clamp(_pixels, _minScrollExtent, _maxScrollExtent);

        if (Math.Abs(oldPixels - _pixels) > 0.0001)
        {
            NotifyListeners();
            return true;
        }

        return minChanged || maxChanged;
    }

    private void SetPixels(double value)
    {
        var overscroll = Physics.ApplyBoundaryConditions(this, value);
        var newPixels = value - overscroll;

        if (Math.Abs(newPixels - _pixels) < 0.0001)
        {
            return;
        }

        _pixels = newPixels;
        NotifyListeners();
    }
}
