using Avalonia;

namespace Flutter.UI;

public static class SizeExtensions
{
    extension(Size size)
    {
        public Size Flipped => new(size.Height, size.Width);

        public bool IsEmpty => size.Width <= 0.0 || size.Height <= 0.0;
    }
}