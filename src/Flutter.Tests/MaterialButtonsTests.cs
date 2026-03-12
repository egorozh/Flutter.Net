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
    public void TextButton_ButtonStyleForegroundOverridesDefault()
    {
        var owner = new BuildOwner();

        var root = new TestRootElement(
            new Theme(
                data: ThemeData.Light,
                child: new TextButton(
                    onPressed: () => { },
                    style: new ButtonStyle(
                        ForegroundColor: MaterialStateProperty<Color?>.All(Colors.ForestGreen)),
                    child: new Text("Styled foreground"))));

        root.Attach(owner);
        root.Mount(parent: null, newSlot: null);
        owner.FlushBuild();

        var paragraph = FindDescendant<RenderParagraph>(RequireRenderObject<RenderObject>(root.ChildElement));
        Assert.NotNull(paragraph);
        Assert.Equal(Colors.ForestGreen, Assert.IsType<SolidColorBrush>(paragraph!.Foreground).Color);
    }

    [Fact]
    public void ElevatedButton_ButtonStyleMinimumSizeOverridesDefault()
    {
        var owner = new BuildOwner();
        var root = new TestRootElement(
            new Theme(
                data: ThemeData.Light,
                child: new ElevatedButton(
                    onPressed: () => { },
                    style: new ButtonStyle(
                        MinimumSize: MaterialStateProperty<Size?>.All(new Size(180, 56))),
                    child: new Text("Styled size"))));

        root.Attach(owner);
        root.Mount(parent: null, newSlot: null);
        owner.FlushBuild();

        var constrainedBox = FindDescendant<RenderConstrainedBox>(RequireRenderObject<RenderObject>(root.ChildElement));
        Assert.NotNull(constrainedBox);
        Assert.Equal(180, constrainedBox!.AdditionalConstraints.MinWidth);
        Assert.Equal(56, constrainedBox.AdditionalConstraints.MinHeight);
    }

    [Fact]
    public void OutlinedButton_ButtonStyleSideOverridesDefault()
    {
        var owner = new BuildOwner();
        var root = new TestRootElement(
            new Theme(
                data: ThemeData.Light,
                child: new OutlinedButton(
                    onPressed: () => { },
                    style: new ButtonStyle(
                        Side: MaterialStateProperty<BorderSide?>.All(new BorderSide(Colors.Goldenrod, 2))),
                    child: new Text("Styled side"))));

        root.Attach(owner);
        root.Mount(parent: null, newSlot: null);
        owner.FlushBuild();

        var decorated = FindDescendant<RenderDecoratedBox>(RequireRenderObject<RenderObject>(root.ChildElement));
        Assert.NotNull(decorated);
        Assert.Equal(new BorderSide(Colors.Goldenrod, 2), decorated!.Decoration.Border);
    }

    [Fact]
    public void TextButton_StyleFrom_AppliesForegroundAndTextStyle()
    {
        var owner = new BuildOwner();
        var root = new TestRootElement(
            new Theme(
                data: ThemeData.Light,
                child: new TextButton(
                    onPressed: () => { },
                    style: TextButton.StyleFrom(
                        foregroundColor: Colors.DarkCyan,
                        textStyle: new TextStyle(
                            FontSize: 18,
                            FontWeight: FontWeight.Bold)),
                    child: new Text("StyleFrom"))));

        root.Attach(owner);
        root.Mount(parent: null, newSlot: null);
        owner.FlushBuild();

        var paragraph = FindDescendant<RenderParagraph>(RequireRenderObject<RenderObject>(root.ChildElement));
        Assert.NotNull(paragraph);
        Assert.Equal(Colors.DarkCyan, Assert.IsType<SolidColorBrush>(paragraph!.Foreground).Color);
        Assert.Equal(18, paragraph.FontSize);
        Assert.Equal(FontWeight.Bold, paragraph.FontWeight);
    }

    [Fact]
    public void ElevatedButton_StyleFrom_UsesDisabledColorOverrides()
    {
        var owner = new BuildOwner();
        var root = new TestRootElement(
            new Theme(
                data: ThemeData.Light,
                child: new ElevatedButton(
                    onPressed: null,
                    style: ElevatedButton.StyleFrom(
                        foregroundColor: Colors.White,
                        backgroundColor: Colors.SeaGreen,
                        disabledForegroundColor: Colors.SlateGray,
                        disabledBackgroundColor: Colors.SaddleBrown),
                    child: new Text("Disabled styleFrom"))));

        root.Attach(owner);
        root.Mount(parent: null, newSlot: null);
        owner.FlushBuild();

        var renderRoot = RequireRenderObject<RenderObject>(root.ChildElement);
        var decorated = FindDescendant<RenderDecoratedBox>(renderRoot);
        var paragraph = FindDescendant<RenderParagraph>(renderRoot);
        Assert.NotNull(decorated);
        Assert.NotNull(paragraph);
        Assert.Equal(Colors.SaddleBrown, decorated!.Decoration.Color);
        Assert.Equal(Colors.SlateGray, Assert.IsType<SolidColorBrush>(paragraph!.Foreground).Color);
    }

    [Fact]
    public void TextButton_LegacyForeground_OverridesStyleFromForeground()
    {
        var owner = new BuildOwner();
        var root = new TestRootElement(
            new Theme(
                data: ThemeData.Light,
                child: new TextButton(
                    onPressed: () => { },
                    foregroundColor: Colors.OrangeRed,
                    style: TextButton.StyleFrom(foregroundColor: Colors.RoyalBlue),
                    child: new Text("Legacy wins"))));

        root.Attach(owner);
        root.Mount(parent: null, newSlot: null);
        owner.FlushBuild();

        var paragraph = FindDescendant<RenderParagraph>(RequireRenderObject<RenderObject>(root.ChildElement));
        Assert.NotNull(paragraph);
        Assert.Equal(Colors.OrangeRed, Assert.IsType<SolidColorBrush>(paragraph!.Foreground).Color);
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
    public void TextButton_PressedOverlayTakesPriorityOverHoverOverlay()
    {
        var owner = new BuildOwner();
        var theme = ThemeData.Light with
        {
            PrimaryColor = Colors.CornflowerBlue
        };

        var root = new TestRootElement(
            new Theme(
                data: theme,
                child: new TextButton(
                    onPressed: () => { },
                    child: new Text("Priority"))));

        root.Attach(owner);
        root.Mount(parent: null, newSlot: null);
        owner.FlushBuild();

        var hoverListener = FindHoverPointerListener(RequireRenderObject<RenderObject>(root.ChildElement));
        Assert.NotNull(hoverListener);
        hoverListener!.HandleEvent(
            new PointerEnterEvent(
                pointer: 11,
                kind: PointerDeviceKind.Mouse,
                position: new Point(10, 10),
                buttons: PointerButtons.None,
                timestampUtc: DateTime.UtcNow),
            new BoxHitTestEntry(hoverListener, new Point(10, 10)));

        owner.FlushBuild();

        var hoveredDecorated = FindDescendant<RenderDecoratedBox>(RequireRenderObject<RenderObject>(root.ChildElement));
        Assert.NotNull(hoveredDecorated);
        Assert.Equal(ApplyOpacity(theme.PrimaryColor, 0.08), hoveredDecorated!.Decoration.Color);

        var interactiveListener = FindInteractivePointerListener(RequireRenderObject<RenderObject>(root.ChildElement));
        Assert.NotNull(interactiveListener);
        interactiveListener!.HandleEvent(
            new PointerDownEvent(
                pointer: 11,
                kind: PointerDeviceKind.Mouse,
                position: new Point(10, 10),
                buttons: PointerButtons.Primary,
                timestampUtc: DateTime.UtcNow),
            new BoxHitTestEntry(interactiveListener, new Point(10, 10)));

        owner.FlushBuild();

        var pressedDecorated = FindDescendant<RenderDecoratedBox>(RequireRenderObject<RenderObject>(root.ChildElement));
        Assert.NotNull(pressedDecorated);
        Assert.Equal(ApplyOpacity(theme.PrimaryColor, 0.10), pressedDecorated!.Decoration.Color);

        interactiveListener = FindInteractivePointerListener(RequireRenderObject<RenderObject>(root.ChildElement));
        Assert.NotNull(interactiveListener);
        interactiveListener!.HandleEvent(
            new PointerUpEvent(
                pointer: 11,
                kind: PointerDeviceKind.Mouse,
                position: new Point(10, 10),
                buttons: PointerButtons.None,
                timestampUtc: DateTime.UtcNow),
            new BoxHitTestEntry(interactiveListener, new Point(10, 10)));

        owner.FlushBuild();

        var releasedDecorated = FindDescendant<RenderDecoratedBox>(RequireRenderObject<RenderObject>(root.ChildElement));
        Assert.NotNull(releasedDecorated);
        Assert.Equal(ApplyOpacity(theme.PrimaryColor, 0.08), releasedDecorated!.Decoration.Color);
    }

    [Fact]
    public void TextButton_HoverOverlayTakesPriorityOverFocusOverlay()
    {
        var owner = new BuildOwner();
        var theme = ThemeData.Light with
        {
            PrimaryColor = Colors.MediumSeaGreen
        };

        var root = new TestRootElement(
            new Theme(
                data: theme,
                child: new TextButton(
                    onPressed: () => { },
                    child: new Text("Focus hover"))));

        root.Attach(owner);
        root.Mount(parent: null, newSlot: null);
        owner.FlushBuild();

        var focusListener = FindFocusPointerListener(RequireRenderObject<RenderObject>(root.ChildElement));
        Assert.NotNull(focusListener);
        focusListener!.HandleEvent(
            new PointerDownEvent(
                pointer: 19,
                kind: PointerDeviceKind.Mouse,
                position: new Point(12, 9),
                buttons: PointerButtons.Primary,
                timestampUtc: DateTime.UtcNow),
            new BoxHitTestEntry(focusListener, new Point(12, 9)));

        owner.FlushBuild();

        var focusedDecorated = FindDescendant<RenderDecoratedBox>(RequireRenderObject<RenderObject>(root.ChildElement));
        Assert.NotNull(focusedDecorated);
        Assert.Equal(ApplyOpacity(theme.PrimaryColor, 0.10), focusedDecorated!.Decoration.Color);

        var hoverListener = FindHoverPointerListener(RequireRenderObject<RenderObject>(root.ChildElement));
        Assert.NotNull(hoverListener);
        hoverListener!.HandleEvent(
            new PointerEnterEvent(
                pointer: 19,
                kind: PointerDeviceKind.Mouse,
                position: new Point(12, 9),
                buttons: PointerButtons.None,
                timestampUtc: DateTime.UtcNow),
            new BoxHitTestEntry(hoverListener, new Point(12, 9)));

        owner.FlushBuild();

        var focusedHoveredDecorated = FindDescendant<RenderDecoratedBox>(RequireRenderObject<RenderObject>(root.ChildElement));
        Assert.NotNull(focusedHoveredDecorated);
        Assert.Equal(ApplyOpacity(theme.PrimaryColor, 0.08), focusedHoveredDecorated!.Decoration.Color);

        hoverListener = FindHoverPointerListener(RequireRenderObject<RenderObject>(root.ChildElement));
        Assert.NotNull(hoverListener);
        hoverListener!.HandleEvent(
            new PointerExitEvent(
                pointer: 19,
                kind: PointerDeviceKind.Mouse,
                position: new Point(120, 9),
                buttons: PointerButtons.None,
                timestampUtc: DateTime.UtcNow),
            new BoxHitTestEntry(hoverListener, new Point(120, 9)));

        owner.FlushBuild();

        var focusOnlyDecorated = FindDescendant<RenderDecoratedBox>(RequireRenderObject<RenderObject>(root.ChildElement));
        Assert.NotNull(focusOnlyDecorated);
        Assert.Equal(ApplyOpacity(theme.PrimaryColor, 0.10), focusOnlyDecorated!.Decoration.Color);
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

    [Fact]
    public void TextButton_PointerClick_DoesNotKeepFocusOverlayAfterPointerUp()
    {
        var owner = new BuildOwner();
        var theme = ThemeData.Light with
        {
            PrimaryColor = Colors.Coral
        };

        var root = new TestRootElement(
            new Theme(
                data: theme,
                child: new TextButton(
                    onPressed: () => { },
                    child: new Text("Pointer focus"))));

        root.Attach(owner);
        root.Mount(parent: null, newSlot: null);
        owner.FlushBuild();

        var renderRoot = RequireRenderObject<RenderObject>(root.ChildElement);
        var interactiveListener = FindInteractivePointerListener(renderRoot);
        var focusListener = FindFocusPointerListener(renderRoot);
        Assert.NotNull(interactiveListener);
        Assert.NotNull(focusListener);

        interactiveListener!.HandleEvent(
            new PointerDownEvent(
                pointer: 17,
                kind: PointerDeviceKind.Mouse,
                position: new Point(12, 10),
                buttons: PointerButtons.Primary,
                timestampUtc: DateTime.UtcNow),
            new BoxHitTestEntry(interactiveListener, new Point(12, 10)));

        focusListener!.HandleEvent(
            new PointerDownEvent(
                pointer: 17,
                kind: PointerDeviceKind.Mouse,
                position: new Point(12, 10),
                buttons: PointerButtons.Primary,
                timestampUtc: DateTime.UtcNow),
            new BoxHitTestEntry(focusListener, new Point(12, 10)));

        owner.FlushBuild();

        interactiveListener = FindInteractivePointerListener(RequireRenderObject<RenderObject>(root.ChildElement));
        Assert.NotNull(interactiveListener);
        interactiveListener!.HandleEvent(
            new PointerUpEvent(
                pointer: 17,
                kind: PointerDeviceKind.Mouse,
                position: new Point(12, 10),
                buttons: PointerButtons.None,
                timestampUtc: DateTime.UtcNow),
            new BoxHitTestEntry(interactiveListener, new Point(12, 10)));

        owner.FlushBuild();

        var decorated = FindDescendant<RenderDecoratedBox>(RequireRenderObject<RenderObject>(root.ChildElement));
        Assert.NotNull(decorated);
        Assert.Null(decorated!.Decoration.Color);
    }

    [Fact]
    public void TextButton_TightWidth_ExpandsInkSplashToFullButtonBounds()
    {
        using var harness = new WidgetRenderHarness(
            new Theme(
                data: ThemeData.Light,
                child: new SizedBox(
                    width: 240,
                    child: new TextButton(
                        onPressed: () => { },
                        child: new Text("Wide button")))));

        harness.Pump(new Size(300, 120));

        var renderRoot = harness.RenderView.Child;
        var splash = FindDescendant<RenderInkSplash>(renderRoot);
        var decorated = FindDescendant<RenderDecoratedBox>(renderRoot);

        Assert.NotNull(splash);
        Assert.NotNull(decorated);
        Assert.Equal(240, decorated!.Size.Width, 3);
        Assert.Equal(240, splash!.Size.Width, 3);
        Assert.Equal(decorated.Size.Height, splash.Size.Height, 3);
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

    private static RenderPointerListener? FindFocusPointerListener(RenderObject? root)
    {
        if (root is null)
        {
            return null;
        }

        if (root is RenderPointerListener listener
            && listener.OnPointerDown != null
            && listener.OnPointerUp == null
            && listener.OnPointerCancel == null
            && listener.OnPointerEnter == null
            && listener.OnPointerExit == null)
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

            result = FindFocusPointerListener(child);
        });

        return result;
    }

    private sealed class WidgetRenderHarness : IDisposable
    {
        private readonly BuildOwner _owner = new();
        private readonly HarnessRootElement _rootElement;
        private readonly PipelineOwner _pipeline;

        public WidgetRenderHarness(Widget rootWidget)
        {
            RenderView = new RenderView();
            _pipeline = new PipelineOwner(RenderView);
            _pipeline.Attach(RenderView);

            _rootElement = new HarnessRootElement(RenderView, rootWidget);
            _rootElement.Attach(_owner);
            _rootElement.Mount(parent: null, newSlot: null);
            _owner.FlushBuild();
        }

        public RenderView RenderView { get; }

        public void Pump(Size size)
        {
            _owner.FlushBuild();
            _pipeline.RequestLayout();
            _pipeline.FlushLayout(size);
            _pipeline.FlushCompositingBits();
            _pipeline.FlushPaint();
        }

        public void Dispose()
        {
            _rootElement.Unmount();
        }

        private sealed class HarnessRootElement : Element, IRenderObjectHost
        {
            private readonly RenderView _renderView;
            private Element? _child;

            public HarnessRootElement(RenderView renderView, Widget widget) : base(widget)
            {
                _renderView = renderView;
            }

            public override RenderObject? RenderObject => _child?.RenderObject;

            internal override Element? RenderObjectAttachingChild => _child;

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

            internal override void ForgetChild(Element child)
            {
                if (ReferenceEquals(_child, child))
                {
                    _child = null;
                }
            }

            internal override void VisitChildren(Action<Element> visitor)
            {
                if (_child != null)
                {
                    visitor(_child);
                }
            }

            public void InsertRenderObjectChild(RenderObject child, object? slot)
            {
                if (slot != null)
                {
                    throw new InvalidOperationException("HarnessRootElement expects null slot.");
                }

                if (child is not RenderBox renderBox)
                {
                    throw new InvalidOperationException("HarnessRootElement can host only RenderBox.");
                }

                _renderView.Child = renderBox;
            }

            public void MoveRenderObjectChild(RenderObject child, object? oldSlot, object? newSlot)
            {
                if (!Equals(oldSlot, newSlot))
                {
                    throw new InvalidOperationException("HarnessRootElement does not support non-null slot moves.");
                }
            }

            public void RemoveRenderObjectChild(RenderObject child, object? slot)
            {
                if (slot != null)
                {
                    throw new InvalidOperationException("HarnessRootElement expects null slot.");
                }

                if (ReferenceEquals(_renderView.Child, child))
                {
                    _renderView.Child = null;
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
        }
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
