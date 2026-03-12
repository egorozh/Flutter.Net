using Avalonia;
using Avalonia.Media;
using Flutter.Material;
using Flutter.Rendering;
using Flutter.Widgets;
using Xunit;

namespace Flutter.Tests;

public sealed class MaterialScaffoldTests
{
    [Fact]
    public void Scaffold_UsesThemeScaffoldBackgroundColor()
    {
        var owner = new BuildOwner();
        var theme = ThemeData.Light with
        {
            ScaffoldBackgroundColor = Colors.Beige
        };

        var root = new TestRootElement(
            new Theme(
                data: theme,
                child: new Scaffold(
                    body: new SizedBox(width: 24, height: 12))));

        root.Attach(owner);
        root.Mount(parent: null, newSlot: null);
        owner.FlushBuild();

        var background = RequireRenderObject<RenderColoredBox>(root.ChildElement);
        Assert.Equal(Colors.Beige, background.Color);
    }

    [Fact]
    public void Scaffold_UsesExplicitBackgroundColorOverride()
    {
        var owner = new BuildOwner();
        var root = new TestRootElement(
            new Theme(
                data: ThemeData.Light with
                {
                    ScaffoldBackgroundColor = Colors.White
                },
                child: new Scaffold(
                    backgroundColor: Colors.Crimson,
                    body: new SizedBox(width: 24, height: 12))));

        root.Attach(owner);
        root.Mount(parent: null, newSlot: null);
        owner.FlushBuild();

        var background = RequireRenderObject<RenderColoredBox>(root.ChildElement);
        Assert.Equal(Colors.Crimson, background.Color);
    }

    [Fact]
    public void Scaffold_WithAppBar_UsesThemePrimaryColorForAppBarBackground()
    {
        var owner = new BuildOwner();
        var theme = ThemeData.Light with
        {
            PrimaryColor = Colors.DarkSlateBlue
        };

        var root = new TestRootElement(
            new Theme(
                data: theme,
                child: new Scaffold(
                    appBar: new AppBar(titleText: "Demo"),
                    body: new SizedBox(width: 24, height: 12))));

        root.Attach(owner);
        root.Mount(parent: null, newSlot: null);
        owner.FlushBuild();

        var scaffoldBackground = RequireRenderObject<RenderColoredBox>(root.ChildElement);
        var contentColumn = Assert.IsType<RenderFlex>(scaffoldBackground.Child);
        var appBarBackground = Assert.IsType<RenderColoredBox>(contentColumn.FirstChild);

        Assert.Equal(Colors.DarkSlateBlue, appBarBackground.Color);
        Assert.NotNull(contentColumn.ChildAfter(appBarBackground));
    }

    [Fact]
    public void AppBar_DefaultTitle_UsesThemeOnPrimaryColor()
    {
        var owner = new BuildOwner();
        var theme = ThemeData.Light with
        {
            PrimaryColor = Colors.DarkSlateBlue,
            OnPrimaryColor = Colors.Bisque
        };

        var root = new TestRootElement(
            new Theme(
                data: theme,
                child: new AppBar(titleText: "Demo")));

        root.Attach(owner);
        root.Mount(parent: null, newSlot: null);
        owner.FlushBuild();

        var appBarBackground = RequireRenderObject<RenderColoredBox>(root.ChildElement);
        Assert.Equal(Colors.DarkSlateBlue, appBarBackground.Color);

        var paragraph = FindDescendant<RenderParagraph>(appBarBackground);
        Assert.NotNull(paragraph);
        Assert.Equal(Colors.Bisque, Assert.IsType<SolidColorBrush>(paragraph!.Foreground).Color);
    }

    private static T RequireRenderObject<T>(Element? element) where T : RenderObject
    {
        Assert.NotNull(element);
        Assert.NotNull(element!.RenderObject);
        return Assert.IsType<T>(element.RenderObject);
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
