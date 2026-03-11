using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Media;
using Flutter.Rendering;
using Flutter.Widgets;

// Dart parity source (reference): dart_sample/lib/sample_gallery_screen.dart; dart_sample/lib/sample_routes.dart (exact sample parity)

namespace Flutter.Net;

internal static class SampleNavigationObservers
{
    public static RouteObserver<PageRoute> PageRoutes { get; } = new();
}

internal static class SampleRoutes
{
    public const string Menu = "/";
    public const string Counter = "/counter";
    public const string Navigator = "/navigator";
    public const string NavigatorDetails = "/navigator/details";
    public const string ListViewSeparated = "/list-separated";
    public const string ListViewFixedExtent = "/list-fixed-extent";
    public const string ListViewReverse = "/list-reverse";
    public const string GridView = "/grid-view";
    public const string CustomSlivers = "/custom-slivers";
    public const string Scrollbar = "/scrollbar";
    public const string EditableText = "/editable-text";
    public const string ProxyWidgets = "/proxy-widgets";
    public const string Align = "/align";
    public const string Stack = "/stack";
    public const string DecoratedBox = "/decorated-box";
    public const string Container = "/container";
    public const string AspectRatio = "/aspect-ratio";
    public const string FractionallySizedBox = "/fractionally-sized-box";
    public const string FittedBox = "/fitted-box";
    public const string UnconstrainedLimitedBox = "/unconstrained-limited-box";
}

internal readonly record struct SampleRouteDefinition(
    string RouteName,
    string Title,
    string Subtitle,
    Func<Widget> Builder);

internal sealed class SampleGalleryScreen : StatelessWidget
{
    private static readonly IReadOnlyList<SampleRouteDefinition> DemoPages =
    [
        new(SampleRoutes.Counter, "Counter", "existing sample", () => new CounterScreen()),
        new(SampleRoutes.Navigator, "Navigator", "named routes + RouteData + stack APIs", () => new NavigatorDemoPage()),
        new(SampleRoutes.ListViewSeparated, "ListView.Separated", "item + separator builder", () => new ListViewSeparatedDemoPage()),
        new(SampleRoutes.ListViewFixedExtent, "ListView fixed extent", "itemExtent + padding", () => new ListViewFixedExtentDemoPage()),
        new(SampleRoutes.ListViewReverse, "ListView reverse", "reverse=true behavior", () => new ListViewReverseDemoPage()),
        new(SampleRoutes.GridView, "GridView + SliverGrid", "delegate-based 2D layout", () => new GridViewDemoPage()),
        new(SampleRoutes.CustomSlivers, "Custom slivers", "SliverPadding + SliverFixedExtentList", () => new CustomSliversDemoPage()),
        new(SampleRoutes.Scrollbar, "Scrollbar", "controller + thumb", () => new ScrollbarDemoPage()),
        new(SampleRoutes.EditableText, "EditableText", "focus + IME + multiline caret", () => new EditableTextDemoPage()),
        new(SampleRoutes.ProxyWidgets, "Proxy widgets", "Opacity + Transform + ClipRect composition", () => new ProxyWidgetsDemoPage()),
        new(SampleRoutes.Align, "Align + Center", "single-child alignment and shrink factors", () => new AlignDemoPage()),
        new(SampleRoutes.Stack, "Stack + Positioned", "multi-child overlay layout", () => new StackDemoPage()),
        new(SampleRoutes.DecoratedBox, "DecoratedBox", "border + radius + fill decoration", () => new DecoratedBoxDemoPage()),
        new(SampleRoutes.Container, "Container", "alignment + margin + constraints + transform", () => new ContainerDemoPage()),
        new(SampleRoutes.AspectRatio, "AspectRatio + Spacer", "tight ratio layout + flex gap", () => new AspectRatioDemoPage()),
        new(SampleRoutes.FractionallySizedBox, "FractionallySizedBox", "fractional constraints + alignment", () => new FractionallySizedBoxDemoPage()),
        new(SampleRoutes.FittedBox, "FittedBox", "box-fit scaling + alignment", () => new FittedBoxDemoPage()),
        new(SampleRoutes.UnconstrainedLimitedBox, "UnconstrainedBox + LimitedBox", "axis unconstraint + unbounded max clamps", () => new UnconstrainedLimitedBoxDemoPage()),
    ];

    private static readonly IReadOnlyDictionary<string, SampleRouteDefinition> DemoPageByRoute =
        DemoPages.ToDictionary(page => page.RouteName, page => page);

    public override Widget Build(BuildContext context)
    {
        return new Navigator(
            onGenerateRoute: BuildRoute,
            observers: [SampleNavigationObservers.PageRoutes],
            initialRouteName: SampleRoutes.Menu);
    }

    private static Route? BuildRoute(RouteSettings settings)
    {
        if (settings.Name == SampleRoutes.Menu)
        {
            return new BuilderPageRoute(
                builder: _ => new SampleMenuPage(DemoPages),
                settings: settings);
        }

        if (settings.Name == SampleRoutes.NavigatorDetails)
        {
            var routeData = settings.Arguments as RouteData
                ?? new RouteData(SampleRoutes.NavigatorDetails, arguments: settings.Arguments);
            return new BuilderPageRoute(
                builder: _ => new SampleDemoPage(
                    title: "Navigator details",
                    subtitle: "RouteData query/arguments + push/pop operations",
                    child: new NavigatorDetailsPage(routeData)),
                settings: settings);
        }

        if (settings.Name != null && DemoPageByRoute.TryGetValue(settings.Name, out var page))
        {
            return new BuilderPageRoute(
                builder: _ => new SampleDemoPage(page, page.Builder()),
                settings: settings);
        }

        return null;
    }
}

internal sealed class SampleMenuPage : StatelessWidget
{
    private readonly IReadOnlyList<SampleRouteDefinition> _pages;

    public SampleMenuPage(IReadOnlyList<SampleRouteDefinition> pages)
    {
        _pages = pages;
    }

    public override Widget Build(BuildContext context)
    {
        return new Container(
            color: Colors.White,
            padding: new Thickness(16),
            child: new Column(
                crossAxisAlignment: CrossAxisAlignment.Stretch,
                spacing: 10,
                children:
                [
                    new Text("Flutter.Net widget pages", fontSize: 24, color: Colors.Black),
                    new Text(
                        "Route-based sample menu. Open page and return via Back button or Esc.",
                        fontSize: 14,
                        color: Colors.DimGray),
                    new Expanded(
                        child: ListView.Builder(
                            itemCount: _pages.Count,
                            padding: new Thickness(0, 8, 0, 8),
                            itemExtent: 56,
                            itemBuilder: (_, index) => BuildPageButton(context, _pages[index]),
                            addAutomaticKeepAlives: false)),
                ]));
    }

    private static Widget BuildPageButton(BuildContext context, SampleRouteDefinition page)
    {
        return new CounterTapButton(
            label: $"{page.Title}  |  {page.Subtitle}",
            onTap: () => Navigator.Of(context).PushNamed(page.RouteName),
            background: Color.Parse("#FFDCE3ED"),
            foreground: Colors.Black,
            fontSize: 12,
            padding: new Thickness(10, 8));
    }
}

internal sealed class SampleDemoPage : StatelessWidget
{
    private readonly string _title;
    private readonly string _subtitle;
    private readonly Widget _child;

    public SampleDemoPage(SampleRouteDefinition page, Widget child)
        : this(page.Title, page.Subtitle, child)
    {
    }

    public SampleDemoPage(string title, string subtitle, Widget child)
    {
        _title = title;
        _subtitle = subtitle;
        _child = child;
    }

    public override Widget Build(BuildContext context)
    {
        return new Container(
            color: Colors.White,
            padding: new Thickness(16),
            child: new Column(
                crossAxisAlignment: CrossAxisAlignment.Stretch,
                spacing: 10,
                children:
                [
                    new Row(
                        spacing: 8,
                        children:
                        [
                            new SizedBox(
                                width: 90,
                                child: new CounterTapButton(
                                    label: "Back",
                                    onTap: () => Navigator.Of(context).MaybePop(),
                                    background: Colors.SteelBlue,
                                    foreground: Colors.White,
                                    fontSize: 12,
                                    padding: new Thickness(10, 8))),
                            new Expanded(
                                child: new Text(_title, fontSize: 22, color: Colors.Black)),
                        ]),
                    new Text(_subtitle, fontSize: 14, color: Colors.DimGray),
                    new Expanded(
                        child: new Container(
                            color: Color.Parse("#FFF7F9FC"),
                            padding: new Thickness(12),
                            child: _child)),
                ]));
    }
}
