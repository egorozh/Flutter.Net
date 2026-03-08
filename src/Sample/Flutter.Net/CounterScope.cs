using System;
using Flutter.Widgets;

// Dart parity source (reference): dart_sample/lib/counter_scope.dart (exact sample parity)

namespace Flutter.Net;

public sealed class CounterScope : InheritedNotifier<CounterAppModel>
{
    public CounterScope(CounterAppModel notifier, Widget child) : base(notifier, child)
    {
    }

    public static CounterAppModel Of(BuildContext context)
    {
        var scope = context.DependOnInherited<CounterScope>()
                    ?? throw new InvalidOperationException("CounterScope not found in widget tree.");
        return scope.Notifier ?? throw new InvalidOperationException("CounterScope notifier is null.");
    }
}
