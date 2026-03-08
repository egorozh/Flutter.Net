using System.Linq;
using Avalonia;
using Avalonia.Media;
using Flutter.Foundation;
using Flutter.Rendering;
using Flutter.Widgets;

namespace Flutter.Net;

public sealed class CounterScreen : StatelessWidget
{
    public override Widget Build(BuildContext context)
    {
        var model = CounterScope.Of(context);

        return new Container(
            color: Colors.White,
            padding: new Thickness(20),
            child: new Column(
                crossAxisAlignment: CrossAxisAlignment.Stretch,
                spacing: 12,
                children:
                [
                    new Text("Flutter Counter", fontSize: 24, color: Colors.Black),
                    new Text($"Count: {model.Count}", fontSize: 18, color: Colors.DarkSlateBlue),
                    new Row(
                        spacing: 12,
                        children:
                        [
                            new Expanded(
                                child: new Button(
                                    label: "-",
                                    onPressed: model.Decrement,
                                    background: Colors.LightGray,
                                    foreground: Colors.Black,
                                    fontSize: 20)),
                            new Expanded(
                                child: new Button(
                                    label: "+",
                                    onPressed: model.Increment,
                                    background: Colors.SkyBlue,
                                    foreground: Colors.Black,
                                    fontSize: 20)),
                        ]),
                    new SizedBox(height: 12),
                    new Text("Keyed List (ValueKey)", fontSize: 18, color: Colors.Black),
                    new Row(
                        spacing: 8,
                        children:
                        [
                            new Expanded(
                                child: new Button(
                                    label: "Reverse",
                                    onPressed: model.ReverseItems,
                                    background: Colors.MediumSeaGreen,
                                    foreground: Colors.White,
                                    fontSize: 13)),
                            new Expanded(
                                child: new Button(
                                    label: "Insert Head",
                                    onPressed: model.InsertHead,
                                    background: Colors.SteelBlue,
                                    foreground: Colors.White,
                                    fontSize: 13)),
                            new Expanded(
                                child: new Button(
                                    label: "Remove Tail",
                                    onPressed: model.Items.Count == 0 ? null : model.RemoveTail,
                                    background: Colors.IndianRed,
                                    foreground: Colors.White,
                                    fontSize: 13)),
                        ]),
                    new Column(
                        spacing: 8,
                        children: [..model.Items.Select(id => new KeyedListItem(id, key: new ValueKey<int>(id)))]),
                    new SizedBox(height: 12),
                    new Text("GlobalKey Reparent", fontSize: 18, color: Colors.Black),
                    new Row(
                        spacing: 8,
                        children:
                        [
                            new Expanded(
                                child: new Container(
                                    color: Color.Parse("#FFE8F5E9"),
                                    padding: new Thickness(8),
                                    child: model.PlaceGlobalOnLeft
                                        ? new MovableBadge(key: new GlobalObjectKey<MovableBadgeState>(model.GlobalBadgeIdentity))
                                        : new Text("left slot", color: Colors.Gray))),
                            new Expanded(
                                child: new Container(
                                    color: Color.Parse("#FFE3F2FD"),
                                    padding: new Thickness(8),
                                    child: !model.PlaceGlobalOnLeft
                                        ? new MovableBadge(key: new GlobalObjectKey<MovableBadgeState>(model.GlobalBadgeIdentity))
                                        : new Text("right slot", color: Colors.Gray))),
                        ]),
                    new Button(
                        label: "Move Global Widget",
                        onPressed: model.ToggleGlobalPlacement,
                        background: Colors.SlateGray,
                        foreground: Colors.White,
                        fontSize: 14),
                ]));
    }
}
