using System.Diagnostics;
using Avalonia;
using Avalonia.Media;

namespace Flutter.Rendering;

public class PaintingContext(DrawingContext ctx)
{
    public DrawingContext Context => ctx;


    /// Paint a child [RenderObject].
    ///
    /// If the child has its own composited layer, the child will be composited
    /// into the layer subtree associated with this painting context. Otherwise,
    /// the child will be painted into the current PictureLayer for this context.
    public void PaintChild(RenderObject child, Point offset)
    {
        // Debug.Assert(() => {
        //     //debugOnProfilePaint?.call(child);
        //     return true;
        // }());

        if (child.IsRepaintBoundary)
        {
            StopRecordingIfNeeded();
            //_compositeChild(child, offset);
            // If a render object was a repaint boundary but no longer is one, this
            // is where the framework managed layer is automatically disposed.
        }
        else if (child._wasRepaintBoundary)
        {
            // Debug.Assert(child._layerHandle.layer is OffsetLayer);
            // child._layerHandle.layer = null;
            child._paintWithContext(this, offset);
        }
        else
        {
            child._paintWithContext(this, offset);
        }
    }

    /// Stop recording to a canvas if recording has started.
    ///
    /// Do not call this function directly: functions in this class will call
    /// this method as needed. This function is called internally to ensure that
    /// recording is stopped before adding layers or finalizing the results of a
    /// paint.
    ///
    /// Subclasses that need to customize how recording to a canvas is performed
    /// should override this method to save the results of the custom canvas
    /// recordings.
    protected void StopRecordingIfNeeded()
    {
        // if (!_isRecording)
        // {
        //     return;
        // }

        // assert(() {
        //     if (debugRepaintRainbowEnabled) {
        //         final Paint paint = Paint()
        //             ..style = PaintingStyle.stroke
        //             ..strokeWidth = 6.0
        //             ..color = debugCurrentRepaintColor.toColor();
        //         canvas.drawRect(estimatedBounds.deflate(3.0), paint);
        //     }
        //     if (debugPaintLayerBordersEnabled) {
        //         final Paint paint = Paint()
        //             ..style = PaintingStyle.stroke
        //             ..strokeWidth = 1.0
        //             ..color = const Color(0xFFFF9800);
        //         canvas.drawRect(estimatedBounds, paint);
        //     }
        //     return true;
        // }());
        // _currentLayer!.picture = _recorder!.endRecording();
        // _currentLayer = null;
        // _recorder = null;
        // _canvas = null;
    }
}