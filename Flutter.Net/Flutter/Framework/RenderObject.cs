using Avalonia.Controls;

namespace Flutter.Net.Flutter.Framework;

public abstract class RenderObjectWidget(Key? key = null) : Widget(key)
{
    protected internal abstract Control CreateRenderObject(IBuildContext context);

    protected internal virtual void UpdateRenderObject(IBuildContext context, Control renderObject)
    {
    }

    protected internal virtual void DidUnmountRenderObject(Control renderObject)
    {
    }
}

public abstract class SingleChildRenderObjectWidget(Key? key = null, Widget? child = null) : RenderObjectWidget(key)
{
    public Widget? Child = child;

    protected internal override Element CreateElement() => new SingleChildRenderObjectElement(this);
}

public class RenderObjectElement(RenderObjectWidget widget) : Element(widget)
{
    private Element? _child;
    private Control? _renderObject;

    internal override Control RenderObject => _renderObject!;

    internal override void Mount(Element? parent)
    {
        base.Mount(parent);

        _renderObject = widget.CreateRenderObject(this);

        base.PerformRebuild(); // clears the "dirty" flag
    }

    protected internal override void Update(Widget newWidget)
    {
        base.Update(newWidget);
        PerformRebuild(); // calls widget.updateRenderObject()
    }

    protected override void PerformRebuild()
    {
        widget.UpdateRenderObject(this, RenderObject);

        base.PerformRebuild();
    }

    internal override void Unmount()
    {
        var oldWidget = widget;

        base.Unmount();
        
        oldWidget.DidUnmountRenderObject(RenderObject);
        //_renderObject!.Dispose();
        _renderObject = null;
    }
}

public class SingleChildRenderObjectElement(SingleChildRenderObjectWidget widget) : RenderObjectElement(widget)
{
    // @override
// void visitChildren(ElementVisitor visitor) {
//     if (_child != null) {
//         visitor(_child!);
//     }
// }
//
// @override
// void forgetChild(Element child) {
//     assert(child == _child);
//     _child = null;
//     super.forgetChild(child);
// }
//
// @override
// void mount(Element? parent, Object? newSlot) {
//     super.mount(parent, newSlot);
//     _child = updateChild(_child, (widget as SingleChildRenderObjectWidget).child, null);
// }
//
// @override
// void update(SingleChildRenderObjectWidget newWidget) {
//     super.update(newWidget);
//     assert(widget == newWidget);
//     _child = updateChild(_child, (widget as SingleChildRenderObjectWidget).child, null);
// }
//
// @override
// void insertRenderObjectChild(RenderObject child, Object? slot) {
//     final RenderObjectWithChildMixin<RenderObject> renderObject =
//         this.renderObject as RenderObjectWithChildMixin<RenderObject>;
//     assert(slot == null);
//     assert(renderObject.debugValidateChild(child));
//     renderObject.child = child;
//     assert(renderObject == this.renderObject);
// }
//
// @override
// void moveRenderObjectChild(RenderObject child, Object? oldSlot, Object? newSlot) {
//     assert(false);
// }
//
// @override
// void removeRenderObjectChild(RenderObject child, Object? slot) {
//     final RenderObjectWithChildMixin<RenderObject> renderObject =
//         this.renderObject as RenderObjectWithChildMixin<RenderObject>;
//     assert(slot == null);
//     assert(renderObject.child == child);
//     renderObject.child = null;
//     assert(renderObject == this.renderObject);
// }
}