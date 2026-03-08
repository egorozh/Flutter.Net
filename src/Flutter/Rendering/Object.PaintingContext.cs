using Avalonia;
using Avalonia.Media;
using Avalonia.Media.TextFormatting;

namespace Flutter.Rendering;

public sealed class PaintingContext
{
    private readonly ContainerLayer _containerLayer;
    private PictureLayer? _currentPictureLayer;

    public PaintingContext(ContainerLayer containerLayer)
    {
        _containerLayer = containerLayer;
    }

    public void PaintChild(RenderObject child, Point offset)
    {
        if (child.IsRepaintBoundary)
        {
            StopRecordingIfNeeded();

            var layer = child._layer as OffsetLayer;
            var shouldRepaint = child.NeedsPaint || layer == null;
            layer ??= new OffsetLayer();
            layer.Offset = offset;
            _containerLayer.Append(layer);
            child._layer = layer;

            if (shouldRepaint)
            {
                layer.RemoveAllChildren();
                var childContext = new PaintingContext(layer);
                child._paintWithContext(childContext, new Point(0, 0));
            }
        }
        else if (child._wasRepaintBoundary)
        {
            child._layer = null;
            child._paintWithContext(this, offset);
        }
        else
        {
            child._paintWithContext(this, offset);
        }
    }

    public void DrawRectangle(IBrush brush, IPen? pen, Rect rect, double radiusX = 0, double radiusY = 0)
    {
        var pictureLayer = EnsurePictureLayer();
        pictureLayer.AddDrawCommand((drawingContext, sceneOffset) =>
        {
            var translatedRect = new Rect(rect.Position + sceneOffset, rect.Size);
            drawingContext.DrawRectangle(brush, pen, translatedRect, radiusX, radiusY);
        });
    }

    public void DrawTextLayout(TextLayout layout, Point point)
    {
        var pictureLayer = EnsurePictureLayer();
        pictureLayer.AddDrawCommand((drawingContext, sceneOffset) => layout.Draw(drawingContext, point + sceneOffset));
    }

    public void PushClipRect(Rect clipRect, Action<PaintingContext> painter)
    {
        StopRecordingIfNeeded();

        var layer = new ClipRectLayer
        {
            ClipRect = clipRect
        };

        _containerLayer.Append(layer);

        var childContext = new PaintingContext(layer);
        painter(childContext);
        childContext.StopRecordingIfNeeded();
    }

    public void PushTransform(Matrix transform, Action<PaintingContext> painter)
    {
        StopRecordingIfNeeded();

        var layer = new TransformLayer
        {
            Transform = transform
        };

        _containerLayer.Append(layer);

        var childContext = new PaintingContext(layer);
        painter(childContext);
        childContext.StopRecordingIfNeeded();
    }

    public void PushOpacity(double opacity, Action<PaintingContext> painter)
    {
        StopRecordingIfNeeded();

        var layer = new OpacityLayer
        {
            Opacity = opacity
        };

        _containerLayer.Append(layer);

        var childContext = new PaintingContext(layer);
        painter(childContext);
        childContext.StopRecordingIfNeeded();
    }

    private PictureLayer EnsurePictureLayer()
    {
        if (_currentPictureLayer != null)
        {
            return _currentPictureLayer;
        }

        _currentPictureLayer = new PictureLayer();
        _containerLayer.Append(_currentPictureLayer);
        return _currentPictureLayer;
    }

    private void StopRecordingIfNeeded()
    {
        _currentPictureLayer = null;
    }
}
