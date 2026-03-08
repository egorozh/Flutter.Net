import 'package:flutter/foundation.dart';

class CounterAppModel extends ChangeNotifier {
  int _count = 0;
  int _nextId = 5;
  bool _placeGlobalOnLeft = true;
  List<int> _items = <int>[1, 2, 3, 4];

  int get count => _count;

  List<int> get items => List<int>.unmodifiable(_items);

  bool get placeGlobalOnLeft => _placeGlobalOnLeft;

  Object get globalBadgeIdentity => _globalBadgeIdentity;
  final Object _globalBadgeIdentity = Object();

  void increment() {
    _count += 1;
    notifyListeners();
  }

  void decrement() {
    _count -= 1;
    notifyListeners();
  }

  void reverseItems() {
    _items = _items.reversed.toList(growable: true);
    notifyListeners();
  }

  void insertHead() {
    _items.insert(0, _nextId++);
    notifyListeners();
  }

  void removeTail() {
    if (_items.isEmpty) {
      return;
    }

    _items.removeLast();
    notifyListeners();
  }

  void toggleGlobalPlacement() {
    _placeGlobalOnLeft = !_placeGlobalOnLeft;
    notifyListeners();
  }
}
