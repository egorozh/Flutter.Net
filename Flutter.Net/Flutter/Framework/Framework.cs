using System;
using System.Collections.Generic;
using Avalonia.Controls;

namespace Flutter.Net.Flutter.Framework;

public class Key
{
    public string Value { get; }
    public Key(string value) => Value = value;
    public override bool Equals(object obj) => obj is Key k && k.Value == Value;
    public override int GetHashCode() => Value?.GetHashCode() ?? 0;
}

// Утилита для замены в списке Avalonia Panel.Children
internal static class Extensions
{
    public static IList<T> MutateReplace<T>(this IList<T> list, int index, T newItem)
    {
        list[index] = newItem;
        return list;
    }
}

public class Column : StatelessWidget
{
    public List<Widget> Children { get; }
    public Column(List<Widget> children, Key key = null) : base(key) => Children = children;

    protected override Widget Build(IBuildContext context)
    {
        var panel = new StackPanel { Orientation = Avalonia.Layout.Orientation.Vertical };
        var element = (Element)context;
        var old = element.Children;
        var next = new List<Element>();
        for (int i = 0; i < Children.Count; i++)
        {
            var childWidget = Children[i];
            Element childElem;
            if (i < old.Count && old[i].CanUpdate(childWidget))
            {
                childElem = old[i];
                childElem.Update(childWidget);
            }
            else
            {
                childElem = childWidget.CreateElement();
                childElem.Mount(element);
            }

            next.Add(childElem);
            panel.Children.Add(childElem.RenderObject);
        }

        element.Children = next;
        return panel;
    }
}

public class Text : StatelessWidget
{
    public string Data { get; }
    public Text(string data, Key key = null) : base(key) => Data = data;
    protected override Widget Build(IBuildContext context) => new TextBlock { Text = Data };
}

public class ElevatedButton : StatelessWidget
{
    public Action OnPressed { get; }
    public Widget Child { get; }

    public ElevatedButton(Action onPressed, Widget child, Key? key = null) : base(key)
    {
        OnPressed = onPressed;
        Child = child;
    }

    protected override Widget Build(IBuildContext context)
    {
        var button = new Button();
        var element = (Element)context;
        var childElem = Child.CreateElement();
        childElem.Mount(element);
        button.Content = childElem.RenderObject;
        button.Click += (_, __) => OnPressed();
        return button;
    }
}