using Avalonia;
using Flutter.Foundation;

namespace Flutter.Rendering;

/// <summary>
/// A render object in a 2D Cartesian coordinate system.
/// </summary>
public abstract class RenderBox : RenderObject
{
    private Size? _size;

    public Size Size
    {
        get => _size ??
               throw new InvalidOperationException(
                   $"RenderBox was not laid out: {GetType()}#{Diagnostics.ShortHash(this)}");
        protected set => _size = value;
    }

    public bool HasSize => _size != null;

    protected override IConstraints Constraints => (BoxConstraints)base.Constraints;
}