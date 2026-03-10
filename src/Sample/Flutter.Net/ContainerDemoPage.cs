using System;
using Avalonia;
using Avalonia.Media;
using Flutter.Rendering;
using Flutter.Widgets;

// Dart parity source (reference): dart_sample/lib/container_demo_page.dart (exact sample parity)

namespace Flutter.Net;

public sealed class ContainerDemoPage : StatefulWidget
{
    public override State CreateState()
    {
        return new ContainerDemoPageState();
    }
}

internal sealed class ContainerDemoPageState : State
{
    private Alignment _alignment = Alignment.Center;
    private bool _withMargin;

    public override Widget Build(BuildContext context)
    {
        var margin = _withMargin ? new Thickness(12, 8, 12, 8) : (Thickness?)null;

        return new Column(
            crossAxisAlignment: CrossAxisAlignment.Stretch,
            spacing: 10,
            children:
            [
                new Text("Container alignment + margin", fontSize: 20, color: Colors.Black),
                new Text(
                    "Switch alignment and margin to verify Container composition behavior.",
                    fontSize: 14,
                    color: Colors.DimGray),
                new Row(
                    spacing: 8,
                    children:
                    [
                        BuildButton("TopLeft", () => SetAlignment(Alignment.TopLeft), width: 96, colorHex: "#FFDCE3ED"),
                        BuildButton("Center", () => SetAlignment(Alignment.Center), width: 96, colorHex: "#FFDCE3ED"),
                        BuildButton("BottomRight", () => SetAlignment(Alignment.BottomRight), width: 112, colorHex: "#FFDCE3ED"),
                    ]),
                new Row(
                    spacing: 8,
                    children:
                    [
                        BuildButton(_withMargin ? "Margin: on" : "Margin: off", ToggleMargin, width: 120, colorHex: "#FFE9F5EC"),
                    ]),
                new Text(
                    $"alignment={AlignmentLabel(_alignment)}, margin={(_withMargin ? "on" : "off")}",
                    fontSize: 12,
                    color: Colors.DarkSlateGray),
                new Container(
                    width: 240,
                    height: 160,
                    color: Color.Parse("#FFE7EDF6"),
                    padding: new Thickness(8),
                    child: new Container(
                        margin: margin,
                        width: 150,
                        height: 90,
                        alignment: _alignment,
                        decoration: new BoxDecoration(
                            Color: Color.Parse("#FFCCE3FF"),
                            Border: new BorderSide(Color.Parse("#FF1D3557"), 2),
                            BorderRadius: BorderRadius.Circular(12)),
                        child: new Container(
                            width: 32,
                            height: 20,
                            color: Color.Parse("#FF1D3557"),
                            child: new Center(
                                child: new Text("C", fontSize: 12, color: Colors.White))))),
            ]);
    }

    private Widget BuildButton(string label, Action onTap, double width, string colorHex)
    {
        return new SizedBox(
            width: width,
            child: new CounterTapButton(
                label: label,
                onTap: onTap,
                background: Color.Parse(colorHex),
                foreground: Colors.Black,
                fontSize: 12,
                padding: new Thickness(10, 8)));
    }

    private void SetAlignment(Alignment alignment)
    {
        SetState(() => _alignment = alignment);
    }

    private void ToggleMargin()
    {
        SetState(() => _withMargin = !_withMargin);
    }

    private static string AlignmentLabel(Alignment alignment)
    {
        if (alignment == Alignment.TopLeft)
        {
            return "topLeft";
        }

        if (alignment == Alignment.Center)
        {
            return "center";
        }

        if (alignment == Alignment.BottomRight)
        {
            return "bottomRight";
        }

        return $"({alignment.X:0.##},{alignment.Y:0.##})";
    }
}
