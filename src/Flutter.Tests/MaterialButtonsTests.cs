using Avalonia;
using Avalonia.Media;
using Flutter.Material;
using Flutter.Rendering;
using Flutter.UI;
using Flutter.Widgets;
using Xunit;

namespace Flutter.Tests;

public sealed class MaterialButtonsTests
{
    [Fact]
    public void TextButton_UsesThemePrimaryColorAsDefaultForeground()
    {
        var owner = new BuildOwner();
        var theme = ThemeData.Light with
        {
            PrimaryColor = Colors.OrangeRed
        };

        var root = new TestRootElement(
            new Theme(
                data: theme,
                child: new TextButton(
                    onPressed: () => { },
                    child: new Text("Tap me"))));

        root.Attach(owner);
        root.Mount(parent: null, newSlot: null);
        owner.FlushBuild();

        var paragraph = FindDescendant<RenderParagraph>(RequireRenderObject<RenderObject>(root.ChildElement));
        Assert.NotNull(paragraph);
        Assert.Equal(Colors.OrangeRed, Assert.IsType<SolidColorBrush>(paragraph!.Foreground).Color);
    }

    [Fact]
    public void TextButton_DefaultMinSize_UsesMaterialBaseline64x40()
    {
        var owner = new BuildOwner();

        var root = new TestRootElement(
            new Theme(
                data: ThemeData.Light,
                child: new TextButton(
                    onPressed: () => { },
                    child: new Text("Min size"))));

        root.Attach(owner);
        root.Mount(parent: null, newSlot: null);
        owner.FlushBuild();

        var constrainedBox = FindDescendant<RenderConstrainedBox>(RequireRenderObject<RenderObject>(root.ChildElement));
        Assert.NotNull(constrainedBox);
        Assert.Equal(64, constrainedBox!.AdditionalConstraints.MinWidth);
        Assert.Equal(40, constrainedBox.AdditionalConstraints.MinHeight);
    }

    [Fact]
    public void ElevatedButton_UsesThemeSurfaceContainerAndPrimaryColorsByDefault()
    {
        var owner = new BuildOwner();
        var theme = ThemeData.Light with
        {
            PrimaryColor = Colors.DarkSlateBlue,
            SurfaceContainerLowColor = Colors.Bisque
        };

        var root = new TestRootElement(
            new Theme(
                data: theme,
                child: new ElevatedButton(
                    onPressed: () => { },
                    child: new Text("Primary"))));

        root.Attach(owner);
        root.Mount(parent: null, newSlot: null);
        owner.FlushBuild();

        var renderRoot = RequireRenderObject<RenderObject>(root.ChildElement);
        var decorated = FindDescendant<RenderDecoratedBox>(renderRoot);
        var paragraph = FindDescendant<RenderParagraph>(renderRoot);

        Assert.NotNull(decorated);
        Assert.Equal(Colors.Bisque, decorated!.Decoration.Color);
        Assert.NotNull(paragraph);
        Assert.Equal(Colors.DarkSlateBlue, Assert.IsType<SolidColorBrush>(paragraph!.Foreground).Color);
    }

    [Fact]
    public void OutlinedButton_UsesThemeOutlineColorForBorderByDefault()
    {
        var owner = new BuildOwner();
        var theme = ThemeData.Light with
        {
            OutlineColor = Colors.CadetBlue
        };

        var root = new TestRootElement(
            new Theme(
                data: theme,
                child: new OutlinedButton(
                    onPressed: () => { },
                    child: new Text("Outline"))));

        root.Attach(owner);
        root.Mount(parent: null, newSlot: null);
        owner.FlushBuild();

        var decorated = FindDescendant<RenderDecoratedBox>(RequireRenderObject<RenderObject>(root.ChildElement));
        Assert.NotNull(decorated);
        Assert.Equal(new BorderSide(Colors.CadetBlue, 1), decorated!.Decoration.Border);
    }

    [Fact]
    public void OutlinedButton_DefaultForegroundUsesThemePrimaryColor()
    {
        var owner = new BuildOwner();
        var theme = ThemeData.Light with
        {
            PrimaryColor = Colors.MediumVioletRed,
            OutlineColor = Colors.CadetBlue
        };

        var root = new TestRootElement(
            new Theme(
                data: theme,
                child: new OutlinedButton(
                    onPressed: () => { },
                    child: new Text("Outline fg"))));

        root.Attach(owner);
        root.Mount(parent: null, newSlot: null);
        owner.FlushBuild();

        var paragraph = FindDescendant<RenderParagraph>(RequireRenderObject<RenderObject>(root.ChildElement));
        Assert.NotNull(paragraph);
        Assert.Equal(Colors.MediumVioletRed, Assert.IsType<SolidColorBrush>(paragraph!.Foreground).Color);
    }

    [Fact]
    public void ElevatedButton_DisabledStateUsesThemeOnSurfaceTones()
    {
        var owner = new BuildOwner();
        var background = Colors.DarkGreen;
        var foreground = Colors.White;
        var theme = ThemeData.Light with
        {
            OnSurfaceColor = Colors.Black
        };

        var root = new TestRootElement(
            new Theme(
                data: theme,
                child: new ElevatedButton(
                    onPressed: null,
                    backgroundColor: background,
                    foregroundColor: foreground,
                    child: new Text("Disabled"))));

        root.Attach(owner);
        root.Mount(parent: null, newSlot: null);
        owner.FlushBuild();

        var renderRoot = RequireRenderObject<RenderObject>(root.ChildElement);
        var decorated = FindDescendant<RenderDecoratedBox>(renderRoot);
        var paragraph = FindDescendant<RenderParagraph>(renderRoot);

        Assert.NotNull(decorated);
        Assert.Equal(ApplyOpacity(theme.OnSurfaceColor, 0.12), decorated!.Decoration.Color);
        Assert.NotNull(paragraph);
        Assert.Equal(ApplyOpacity(theme.OnSurfaceColor, 0.38), Assert.IsType<SolidColorBrush>(paragraph!.Foreground).Color);
    }

    [Fact]
    public void ElevatedButton_PointerPressedStateDarkensBackgroundUntilPointerUp()
    {
        var owner = new BuildOwner();
        var background = Colors.SteelBlue;

        var root = new TestRootElement(
            new Theme(
                data: ThemeData.Light,
                child: new ElevatedButton(
                    onPressed: () => { },
                    backgroundColor: background,
                    child: new Text("Press"))));

        root.Attach(owner);
        root.Mount(parent: null, newSlot: null);
        owner.FlushBuild();

        var renderRoot = RequireRenderObject<RenderObject>(root.ChildElement);
        var pointerListener = FindInteractivePointerListener(renderRoot);
        Assert.NotNull(pointerListener);
        pointerListener!.HandleEvent(
            new PointerDownEvent(
                pointer: 1,
                kind: PointerDeviceKind.Mouse,
                position: new Point(8, 8),
                buttons: PointerButtons.Primary,
                timestampUtc: DateTime.UtcNow),
            new BoxHitTestEntry(pointerListener, new Point(8, 8)));

        owner.FlushBuild();

        var pressedRoot = RequireRenderObject<RenderObject>(root.ChildElement);
        var pressedDecoration = FindDescendant<RenderDecoratedBox>(pressedRoot);
        Assert.NotNull(pressedDecoration);
        Assert.NotEqual(background, pressedDecoration!.Decoration.Color);

        pointerListener = FindInteractivePointerListener(pressedRoot);
        Assert.NotNull(pointerListener);
        pointerListener!.HandleEvent(
            new PointerUpEvent(
                pointer: 1,
                kind: PointerDeviceKind.Mouse,
                position: new Point(8, 8),
                buttons: PointerButtons.None,
                timestampUtc: DateTime.UtcNow),
            new BoxHitTestEntry(pointerListener, new Point(8, 8)));

        owner.FlushBuild();

        var releasedDecoration = FindDescendant<RenderDecoratedBox>(RequireRenderObject<RenderObject>(root.ChildElement));
        Assert.NotNull(releasedDecoration);
        Assert.Equal(background, releasedDecoration!.Decoration.Color);
    }

    [Fact]
    public void TextButton_HoverStateAppliesOverlayUntilExit()
    {
        var owner = new BuildOwner();
        var theme = ThemeData.Light with
        {
            PrimaryColor = Colors.IndianRed
        };

        var root = new TestRootElement(
            new Theme(
                data: theme,
                child: new TextButton(
                    onPressed: () => { },
                    child: new Text("Hover"))));

        root.Attach(owner);
        root.Mount(parent: null, newSlot: null);
        owner.FlushBuild();

        var initialDecorated = FindDescendant<RenderDecoratedBox>(RequireRenderObject<RenderObject>(root.ChildElement));
        Assert.NotNull(initialDecorated);
        Assert.Null(initialDecorated!.Decoration.Color);

        var hoverListener = FindHoverPointerListener(RequireRenderObject<RenderObject>(root.ChildElement));
        Assert.NotNull(hoverListener);
        hoverListener!.HandleEvent(
            new PointerEnterEvent(
                pointer: 1,
                kind: PointerDeviceKind.Mouse,
                position: new Point(8, 8),
                buttons: PointerButtons.None,
                timestampUtc: DateTime.UtcNow),
            new BoxHitTestEntry(hoverListener, new Point(8, 8)));

        owner.FlushBuild();

        var hoveredDecorated = FindDescendant<RenderDecoratedBox>(RequireRenderObject<RenderObject>(root.ChildElement));
        Assert.NotNull(hoveredDecorated);
        Assert.Equal(ApplyOpacity(theme.PrimaryColor, 0.08), hoveredDecorated!.Decoration.Color);

        hoverListener = FindHoverPointerListener(RequireRenderObject<RenderObject>(root.ChildElement));
        Assert.NotNull(hoverListener);
        hoverListener!.HandleEvent(
            new PointerExitEvent(
                pointer: 1,
                kind: PointerDeviceKind.Mouse,
                position: new Point(120, 8),
                buttons: PointerButtons.None,
                timestampUtc: DateTime.UtcNow),
            new BoxHitTestEntry(hoverListener, new Point(120, 8)));

        owner.FlushBuild();

        var exitedDecorated = FindDescendant<RenderDecoratedBox>(RequireRenderObject<RenderObject>(root.ChildElement));
        Assert.NotNull(exitedDecorated);
        Assert.Null(exitedDecorated!.Decoration.Color);
    }

    [Fact]
    public void TextButton_PointerDownStartsInkSplashRender()
    {
        var owner = new BuildOwner();
        var theme = ThemeData.Light with
        {
            PrimaryColor = Colors.Teal
        };

        var root = new TestRootElement(
            new Theme(
                data: theme,
                child: new TextButton(
                    onPressed: () => { },
                    child: new Text("Splash"))));

        root.Attach(owner);
        root.Mount(parent: null, newSlot: null);
        owner.FlushBuild();

        var initialSplash = FindDescendant<RenderInkSplash>(RequireRenderObject<RenderObject>(root.ChildElement));
        Assert.NotNull(initialSplash);
        Assert.Null(initialSplash!.SplashColor);
        Assert.Equal(0, initialSplash.SplashProgress);

        var pointerListener = FindInteractivePointerListener(RequireRenderObject<RenderObject>(root.ChildElement));
        Assert.NotNull(pointerListener);
        pointerListener!.HandleEvent(
            new PointerDownEvent(
                pointer: 13,
                kind: PointerDeviceKind.Mouse,
                position: new Point(16, 12),
                buttons: PointerButtons.Primary,
                timestampUtc: DateTime.UtcNow),
            new BoxHitTestEntry(pointerListener, new Point(16, 12)));

        owner.FlushBuild();

        var activeSplash = FindDescendant<RenderInkSplash>(RequireRenderObject<RenderObject>(root.ChildElement));
        Assert.NotNull(activeSplash);
        Assert.NotNull(activeSplash!.SplashColor);
        Assert.Equal(new Point(16, 12), activeSplash.SplashOrigin);
    }

    [Fact]
    public void TextButton_UsesRoundedClipForInkSplash()
    {
        var owner = new BuildOwner();
        var root = new TestRootElement(
            new Theme(
                data: ThemeData.Light,
                child: new TextButton(
                    onPressed: () => { },
                    child: new Text("Rounded splash"))));

        root.Attach(owner);
        root.Mount(parent: null, newSlot: null);
        owner.FlushBuild();

        var renderRoot = RequireRenderObject<RenderObject>(root.ChildElement);
        var clip = FindDescendant<RenderClipRRect>(renderRoot);
        var splash = FindDescendant<RenderInkSplash>(renderRoot);

        Assert.NotNull(clip);
        Assert.Equal(BorderRadius.Circular(20), clip!.BorderRadius);
        Assert.NotNull(splash);
        Assert.False(splash!.ClipToBounds);
    }

    private static T RequireRenderObject<T>(Element? element) where T : RenderObject
    {
        Assert.NotNull(element);
        Assert.NotNull(element!.RenderObject);
        return Assert.IsAssignableFrom<T>(element.RenderObject);
    }

    private static T? FindDescendant<T>(RenderObject? root) where T : RenderObject
    {
        if (root is null)
        {
            return null;
        }

        if (root is T match)
        {
            return match;
        }

        T? result = null;
        root.VisitChildren(child =>
        {
            if (result is not null)
            {
                return;
            }

            result = FindDescendant<T>(child);
        });

        return result;
    }

    private static RenderPointerListener? FindInteractivePointerListener(RenderObject? root)
    {
        if (root is null)
        {
            return null;
        }

        if (root is RenderPointerListener listener
            && listener.OnPointerDown != null
            && listener.OnPointerUp != null)
        {
            return listener;
        }

        RenderPointerListener? result = null;
        root.VisitChildren(child =>
        {
            if (result is not null)
            {
                return;
            }

            result = FindInteractivePointerListener(child);
        });

        return result;
    }

    private static RenderPointerListener? FindHoverPointerListener(RenderObject? root)
    {
        if (root is null)
        {
            return null;
        }

        if (root is RenderPointerListener listener
            && listener.OnPointerEnter != null
            && listener.OnPointerExit != null)
        {
            return listener;
        }

        RenderPointerListener? result = null;
        root.VisitChildren(child =>
        {
            if (result is not null)
            {
                return;
            }

            result = FindHoverPointerListener(child);
        });

        return result;
    }

    private static Color ApplyOpacity(Color color, double opacity)
    {
        var alpha = (byte)Math.Clamp((int)(255 * opacity), 0, 255);
        return Color.FromArgb(alpha, color.R, color.G, color.B);
    }

    private sealed class TestRootElement : Element, IRenderObjectHost
    {
        private Element? _child;

        public TestRootElement(Widget widget) : base(widget)
        {
        }

        public Element? ChildElement => _child;

        protected override void OnMount()
        {
            base.OnMount();
            Rebuild();
        }

        internal override void Rebuild()
        {
            Dirty = false;
            _child = UpdateChild(_child, Widget, Slot);
        }

        internal override void Update(Widget newWidget)
        {
            base.Update(newWidget);
            Rebuild();
        }

        internal override void VisitChildren(Action<Element> visitor)
        {
            if (_child != null)
            {
                visitor(_child);
            }
        }

        internal override void ForgetChild(Element child)
        {
            if (ReferenceEquals(_child, child))
            {
                _child = null;
            }
        }

        internal override void Unmount()
        {
            if (_child != null)
            {
                UnmountChild(_child);
                _child = null;
            }

            base.Unmount();
        }

        public void InsertRenderObjectChild(RenderObject child, object? slot)
        {
            if (slot != null)
            {
                throw new InvalidOperationException("TestRootElement expects null slot.");
            }
        }

        public void MoveRenderObjectChild(RenderObject child, object? oldSlot, object? newSlot)
        {
            if (!Equals(oldSlot, newSlot))
            {
                throw new InvalidOperationException("TestRootElement does not support slot moves.");
            }
        }

        public void RemoveRenderObjectChild(RenderObject child, object? slot)
        {
            if (slot != null)
            {
                throw new InvalidOperationException("TestRootElement expects null slot.");
            }
        }
    }
}
