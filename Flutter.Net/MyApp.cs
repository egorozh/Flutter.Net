using Flutter.Widgets;

namespace Flutter.Net;

public readonly record struct MyApp : IStatelessWidget
{
    public IWidget Build(IBuildContext context)
    {
        return new Column([
            new Text("Hello, Avalonia!"),
            new CounterPage()
        ]);
    }
}