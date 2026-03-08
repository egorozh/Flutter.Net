using System.Collections.Generic;
using Avalonia;
using Avalonia.Media;
using Flutter.Rendering;
using Flutter.Widgets;

namespace Flutter.Net;

internal enum SamplePageId
{
    Counter,
    ListViewSeparated,
    ListViewFixedExtent,
    ListViewReverse,
    GridView,
    CustomSlivers,
    Scrollbar
}

internal readonly record struct SamplePageDefinition(
    SamplePageId Id,
    string Title,
    string Subtitle);

internal sealed class SampleGalleryScreen : StatelessWidget
{
    private static readonly IReadOnlyList<SamplePageDefinition> PageDefinitions =
    [
        new(SamplePageId.Counter, "Counter", "existing sample"),
        new(SamplePageId.ListViewSeparated, "ListView.Separated", "item + separator builder"),
        new(SamplePageId.ListViewFixedExtent, "ListView fixed extent", "itemExtent + padding"),
        new(SamplePageId.ListViewReverse, "ListView reverse", "reverse=true behavior"),
        new(SamplePageId.GridView, "GridView + SliverGrid", "delegate-based 2D layout"),
        new(SamplePageId.CustomSlivers, "Custom slivers", "SliverPadding + SliverFixedExtentList"),
        new(SamplePageId.Scrollbar, "Scrollbar", "controller + thumb"),
    ];

    public override Widget Build(BuildContext context)
    {
        return new Navigator(
            initialRoute: new BuilderPageRoute(
                builder: _ => new SampleMenuPage(PageDefinitions)));
    }
}

internal sealed class SampleMenuPage : StatelessWidget
{
    private readonly IReadOnlyList<SamplePageDefinition> _pages;

    public SampleMenuPage(IReadOnlyList<SamplePageDefinition> pages)
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

    private static Widget BuildPageButton(BuildContext context, SamplePageDefinition page)
    {
        return new CounterTapButton(
            label: $"{page.Title}  |  {page.Subtitle}",
            onTap: () => OpenPage(context, page),
            background: Color.Parse("#FFDCE3ED"),
            foreground: Colors.Black,
            fontSize: 12,
            padding: new Thickness(10, 8));
    }

    private static void OpenPage(BuildContext context, SamplePageDefinition page)
    {
        Navigator.Of(context).Push(
            new BuilderPageRoute(
                builder: _ => new SampleDemoPage(page, BuildDemoContent(page.Id)),
                settings: new RouteSettings(Name: page.Id.ToString())));
    }

    private static Widget BuildDemoContent(SamplePageId pageId)
    {
        return pageId switch
        {
            SamplePageId.Counter => new CounterScreen(),
            SamplePageId.ListViewSeparated => new ListViewSeparatedDemoPage(),
            SamplePageId.ListViewFixedExtent => new ListViewFixedExtentDemoPage(),
            SamplePageId.ListViewReverse => new ListViewReverseDemoPage(),
            SamplePageId.GridView => new GridViewDemoPage(),
            SamplePageId.CustomSlivers => new CustomSliversDemoPage(),
            SamplePageId.Scrollbar => new ScrollbarDemoPage(),
            _ => new SizedBox()
        };
    }
}

internal sealed class SampleDemoPage : StatelessWidget
{
    private readonly SamplePageDefinition _page;
    private readonly Widget _child;

    public SampleDemoPage(SamplePageDefinition page, Widget child)
    {
        _page = page;
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
                                child: new Text(_page.Title, fontSize: 22, color: Colors.Black)),
                        ]),
                    new Text(_page.Subtitle, fontSize: 14, color: Colors.DimGray),
                    new Expanded(
                        child: new Container(
                            color: Color.Parse("#FFF7F9FC"),
                            padding: new Thickness(12),
                            child: _child)),
                ]));
    }
}
