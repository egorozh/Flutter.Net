using Flutter.Net.Flutter.Framework;

namespace Flutter.Net.Flutter.App;

public readonly record struct MyApp : IStatelessWidget
{
    public IWidget Build(IBuildContext context)
    {
        return new Column([
            new Text("Hello, Avalonia!"),
            new CounterWidget()
        ]);
    }
}

public readonly record struct CounterWidget : IStatefulWidget
{
    public State CreateState() => new CounterState();
}

public class CounterState : State
{
    private int _count;

    public override IWidget Build(IBuildContext context)
    {
        return new Column([
            new Text($"Counter: {_count}"),
            new Button("Increment", Increment)
        ]);
    }

    private void Increment() => SetState(() => _count++);
}