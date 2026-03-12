using Avalonia;
using Avalonia.Media;
using Flutter.Foundation;
using Flutter.Rendering;
using Flutter.Widgets;

namespace Flutter.Material;

// Dart parity source (reference): flutter/packages/flutter/lib/src/material/scaffold.dart; flutter/packages/flutter/lib/src/material/app_bar.dart (approximate)

public sealed class Scaffold : StatelessWidget
{
    public Scaffold(
        Widget body,
        AppBar? appBar = null,
        Widget? floatingActionButton = null,
        Widget? bottomNavigationBar = null,
        Color? backgroundColor = null,
        Key? key = null) : base(key)
    {
        Body = body;
        AppBar = appBar;
        FloatingActionButton = floatingActionButton;
        BottomNavigationBar = bottomNavigationBar;
        BackgroundColor = backgroundColor;
    }

    public Widget Body { get; }

    public AppBar? AppBar { get; }

    public Widget? FloatingActionButton { get; }

    public Widget? BottomNavigationBar { get; }

    public Color? BackgroundColor { get; }

    public override Widget Build(BuildContext context)
    {
        var theme = Theme.Of(context);
        var effectiveBackground = BackgroundColor ?? theme.ScaffoldBackgroundColor;

        var children = new List<Widget>();
        if (AppBar != null)
        {
            children.Add(AppBar);
        }

        children.Add(new Expanded(child: Body));

        if (BottomNavigationBar != null)
        {
            children.Add(BottomNavigationBar);
        }

        Widget content = new Column(
            crossAxisAlignment: CrossAxisAlignment.Stretch,
            children: children);

        if (FloatingActionButton != null)
        {
            content = new Stack(
                fit: StackFit.Expand,
                children:
                [
                    content,
                    new Positioned(
                        right: 16,
                        bottom: 16,
                        child: FloatingActionButton),
                ]);
        }

        return new Container(
            color: effectiveBackground,
            child: content);
    }
}

public sealed class AppBar : StatelessWidget
{
    public AppBar(
        string? titleText = null,
        Widget? title = null,
        Widget? leading = null,
        double? leadingWidth = null,
        IReadOnlyList<Widget>? actions = null,
        double toolbarHeight = 56,
        Thickness? padding = null,
        Color? backgroundColor = null,
        Color? foregroundColor = null,
        Key? key = null) : base(key)
    {
        if (double.IsNaN(toolbarHeight) || double.IsInfinity(toolbarHeight) || toolbarHeight <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(toolbarHeight), "Toolbar height must be positive and finite.");
        }

        if (leadingWidth.HasValue && (double.IsNaN(leadingWidth.Value) || double.IsInfinity(leadingWidth.Value) || leadingWidth.Value <= 0))
        {
            throw new ArgumentOutOfRangeException(nameof(leadingWidth), "Leading width must be positive and finite.");
        }

        TitleText = titleText;
        Title = title;
        Leading = leading;
        LeadingWidth = leadingWidth;
        Actions = actions ?? Array.Empty<Widget>();
        ToolbarHeight = toolbarHeight;
        Padding = padding;
        BackgroundColor = backgroundColor;
        ForegroundColor = foregroundColor;
    }

    public string? TitleText { get; }

    public Widget? Title { get; }

    public Widget? Leading { get; }

    public double? LeadingWidth { get; }

    public IReadOnlyList<Widget> Actions { get; }

    public double ToolbarHeight { get; }

    public Thickness? Padding { get; }

    public Color? BackgroundColor { get; }

    public Color? ForegroundColor { get; }

    public override Widget Build(BuildContext context)
    {
        var theme = Theme.Of(context);
        var effectiveBackground = BackgroundColor ?? theme.PrimaryColor;
        var effectiveForeground = ForegroundColor ?? theme.OnPrimaryColor;

        var titleWidget = Title ?? BuildDefaultTitle(effectiveForeground);

        var rowChildren = new List<Widget>();
        if (Leading != null)
        {
            rowChildren.Add(
                new SizedBox(
                    width: LeadingWidth ?? 56,
                    child: new Center(child: Leading)));
        }

        rowChildren.Add(new Expanded(child: titleWidget));

        if (Actions.Count > 0)
        {
            rowChildren.Add(new Row(
                spacing: 8,
                children: Actions));
        }

        return new Container(
            color: effectiveBackground,
            padding: Padding ?? new Thickness(16, 0, 16, 0),
            child: new SizedBox(
                height: ToolbarHeight,
                child: new Row(
                    crossAxisAlignment: CrossAxisAlignment.Center,
                    spacing: 12,
                    children: rowChildren)));
    }

    private Widget BuildDefaultTitle(Color foreground)
    {
        if (string.IsNullOrEmpty(TitleText))
        {
            return new SizedBox();
        }

        return new Text(
            TitleText,
            color: foreground,
            fontSize: 20,
            fontWeight: FontWeight.SemiBold);
    }
}
