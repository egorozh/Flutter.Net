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
        bool? centerTitle = null,
        double? titleSpacing = null,
        TextStyle? toolbarTextStyle = null,
        TextStyle? titleTextStyle = null,
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

        if (titleSpacing.HasValue && (double.IsNaN(titleSpacing.Value) || double.IsInfinity(titleSpacing.Value) || titleSpacing.Value < 0))
        {
            throw new ArgumentOutOfRangeException(nameof(titleSpacing), "Title spacing must be non-negative and finite.");
        }

        TitleText = titleText;
        Title = title;
        Leading = leading;
        LeadingWidth = leadingWidth;
        Actions = actions ?? Array.Empty<Widget>();
        CenterTitle = centerTitle;
        TitleSpacing = titleSpacing;
        ToolbarTextStyle = toolbarTextStyle;
        TitleTextStyle = titleTextStyle;
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

    public bool? CenterTitle { get; }

    public double? TitleSpacing { get; }

    public TextStyle? ToolbarTextStyle { get; }

    public TextStyle? TitleTextStyle { get; }

    public double ToolbarHeight { get; }

    public Thickness? Padding { get; }

    public Color? BackgroundColor { get; }

    public Color? ForegroundColor { get; }

    public override Widget Build(BuildContext context)
    {
        var theme = Theme.Of(context);
        var effectiveBackground = BackgroundColor ?? theme.PrimaryColor;
        var effectiveForeground = ForegroundColor ?? theme.OnPrimaryColor;
        var effectiveCenterTitle = ResolveEffectiveCenterTitle(theme);
        var effectiveTitleSpacing = TitleSpacing ?? theme.AppBarTheme.TitleSpacing ?? 16;
        var effectiveToolbarTextStyle = ResolveToolbarTextStyle(theme, effectiveForeground);
        var effectiveTitleTextStyle = ResolveTitleTextStyle(theme, effectiveForeground);

        var titleWidget = (Widget)new DefaultTextStyle(
            style: effectiveTitleTextStyle,
            child: Title ?? BuildDefaultTitle());
        var middle = (Widget)new Padding(
            insets: new Thickness(effectiveTitleSpacing, 0, effectiveTitleSpacing, 0),
            child: effectiveCenterTitle
                ? new Center(child: titleWidget)
                : titleWidget);

        var rowChildren = new List<Widget>();
        var effectiveLeadingWidth = LeadingWidth ?? 56;
        if (Leading != null)
        {
            rowChildren.Add(
                new SizedBox(
                    width: effectiveLeadingWidth,
                    child: new Center(child: Leading)));
        }

        rowChildren.Add(new Expanded(child: middle));

        if (Actions.Count > 0)
        {
            rowChildren.Add(new Row(
                spacing: 8,
                children: Actions));
        }
        else if (effectiveCenterTitle && Leading != null)
        {
            // Reserve symmetric trailing space when centering title without explicit actions.
            rowChildren.Add(new SizedBox(width: effectiveLeadingWidth));
        }

        return new Container(
            color: effectiveBackground,
            padding: Padding ?? new Thickness(16, 0, 16, 0),
            child: new SizedBox(
                height: ToolbarHeight,
                child: new DefaultTextStyle(
                    style: effectiveToolbarTextStyle,
                    child: new Row(
                        crossAxisAlignment: CrossAxisAlignment.Center,
                        spacing: 0,
                        children: rowChildren))));
    }

    private bool ResolveEffectiveCenterTitle(ThemeData theme)
    {
        if (CenterTitle.HasValue)
        {
            return CenterTitle.Value;
        }

        if (theme.AppBarTheme.CenterTitle.HasValue)
        {
            return theme.AppBarTheme.CenterTitle.Value;
        }

        return ResolvePlatformDefaultCenterTitle(theme.Platform);
    }

    private bool ResolvePlatformDefaultCenterTitle(TargetPlatform platform)
    {
        if (platform is TargetPlatform.IOS or TargetPlatform.MacOS)
        {
            return Actions.Count < 2;
        }

        return false;
    }

    private TextStyle ResolveToolbarTextStyle(ThemeData theme, Color effectiveForeground)
    {
        var baseStyle = theme.TextTheme.BodyMedium with
        {
            Color = effectiveForeground,
        };

        var overrideStyle = ToolbarTextStyle ?? theme.AppBarTheme.ToolbarTextStyle;
        return ComposeTextStyle(baseStyle, overrideStyle);
    }

    private TextStyle ResolveTitleTextStyle(ThemeData theme, Color effectiveForeground)
    {
        var baseStyle = theme.TextTheme.TitleLarge with
        {
            Color = effectiveForeground,
        };

        var overrideStyle = TitleTextStyle ?? theme.AppBarTheme.TitleTextStyle;
        return ComposeTextStyle(baseStyle, overrideStyle);
    }

    private static TextStyle ComposeTextStyle(TextStyle baseStyle, TextStyle? overrideStyle)
    {
        if (overrideStyle is null)
        {
            return baseStyle;
        }

        return baseStyle with
        {
            FontFamily = overrideStyle.FontFamily ?? baseStyle.FontFamily,
            FontSize = overrideStyle.FontSize ?? baseStyle.FontSize,
            Color = overrideStyle.Color ?? baseStyle.Color,
            FontWeight = overrideStyle.FontWeight ?? baseStyle.FontWeight,
            FontStyle = overrideStyle.FontStyle ?? baseStyle.FontStyle,
            Height = overrideStyle.Height ?? baseStyle.Height,
            LetterSpacing = overrideStyle.LetterSpacing ?? baseStyle.LetterSpacing,
        };
    }

    private Widget BuildDefaultTitle()
    {
        if (string.IsNullOrEmpty(TitleText))
        {
            return new SizedBox();
        }

        return new Text(TitleText);
    }
}
