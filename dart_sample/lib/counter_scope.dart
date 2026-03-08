import 'package:flutter/widgets.dart';

import 'counter_app_model.dart';

class CounterScope extends InheritedNotifier<CounterAppModel> {
  const CounterScope({
    super.key,
    required CounterAppModel super.notifier,
    required super.child,
  });

  static CounterAppModel of(BuildContext context) {
    final scope = context.dependOnInheritedWidgetOfExactType<CounterScope>();
    if (scope == null || scope.notifier == null) {
      throw StateError('CounterScope not found in widget tree.');
    }

    return scope.notifier!;
  }
}
