using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Media;
using Flutter.Foundation;
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

    public SampleGalleryScreen(
        SamplePageId currentPage,
        Action<SamplePageId> onPageSelected,
        Key? key = null) : base(key)
    {
        CurrentPage = currentPage;
        OnPageSelected = onPageSelected;
    }

    public SamplePageId CurrentPage { get; }

    public Action<SamplePageId> OnPageSelected { get; }

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
                        "Hardcoded pages for new widgets. Navigator will be added later.",
                        fontSize: 14,
                        color: Colors.DimGray),
                    new SizedBox(
                        height: 56,
                        child: new SingleChildScrollView(
                            scrollDirection: Axis.Horizontal,
                            child: new Row(
                                spacing: 8,
                                children: [..PageDefinitions.Select(BuildPageButton)]))),
                    new Expanded(
                        child: new Container(
                            color: Color.Parse("#FFF7F9FC"),
                            padding: new Thickness(12),
                            child: BuildCurrentPage())),
                ]));
    }

    private Widget BuildPageButton(SamplePageDefinition page)
    {
        var selected = page.Id == CurrentPage;
        return new SizedBox(
            width: 220,
            child: new CounterTapButton(
                label: $"{page.Title}\n{page.Subtitle}",
                onTap: () => OnPageSelected(page.Id),
                background: selected ? Colors.SteelBlue : Color.Parse("#FFDCE3ED"),
                foreground: selected ? Colors.White : Colors.Black,
                fontSize: 12,
                padding: new Thickness(10, 8)));
    }

    private Widget BuildCurrentPage()
    {
        return CurrentPage switch
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
