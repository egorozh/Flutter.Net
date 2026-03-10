# Test Matrix

Purpose: map framework areas to existing test coverage and identify common gaps quickly.

## Coverage Map

| Area | Primary tests | What is covered | Typical gap to watch |
| --- | --- | --- | --- |
| Element lifecycle and reconciliation | `src/Flutter.Tests/ElementLifecycleTests.cs` | Keyed reorder, global key reparent/activation/disposal behavior, mixed keyed/unkeyed sibling reorder, and nested multi-parent mixed keyed/unkeyed reorder (keyed retention + moved unkeyed disposal). | Deep stress cases with rapid multi-frame updates that combine nested mixed keyed/unkeyed groups and `forgottenChildren` paths. |
| Inherited dependencies | `src/Flutter.Tests/InheritedWidgetTests.cs`, `src/Flutter.Tests/InheritedModelTests.cs`, `src/Flutter.Tests/InheritedNotifierTests.cs` | Dependent registration and selective rebuild semantics. | Edge cases around deep tree shadowing plus dynamic notifier swaps. |
| Frame and scheduler flow | `src/Flutter.Tests/FramePipelineTests.cs` | Begin/draw/post-frame order, persistent callbacks, build scheduling in frame. | Long-running callback interactions with host-level visibility transitions. |
| Render object parity | `src/Flutter.Tests/RenderingParityTests.cs` | Constraint normalization, relayout boundary behavior, root relayout triggers, failed-layout exception propagation with dirty-state preservation, `BoxConstraints.Tighten` clamp behavior for out-of-range requests, and no-op invalidation guards in proxy/flex property setters. | Rare constraint edge cases across deep mixed proxy/flex chains. |
| Compositing and layers | `src/Flutter.Tests/CompositingLayerTests.cs`, `src/Flutter.Tests/LayerV2Tests.cs` | Repaint boundary behavior, layer reuse/property updates, layer tree structure, no-op `RenderColoredBox` color updates, and no-op `ReplaceRootLayer` handling. | Layer churn under repeated boundary toggles in large trees. |
| Semantics tree | `src/Flutter.Tests/SemanticsTreeTests.cs` | Actions, merge/split, transform/clip, dirty propagation and id reuse contracts. | Host accessibility bridge integration and end-to-end assistive flows. |
| Gestures and hit testing | `src/Flutter.Tests/GesturePipelineTests.cs` | Transform/clip hit testing, recognizer dispatch, arena conflict outcomes, pointer signals. | Multi-pointer gesture interactions and cancellation races. |
| Navigation | `src/Flutter.Tests/NavigationTests.cs` | Push/pop/replace/remove APIs, named routes, observers, route data semantics. | Host back integration across nested navigators. |
| Focus and keyboard flow | `src/Flutter.Tests/FocusTests.cs` | Focus ownership transitions, primary focus bookkeeping, autofocus, key callback dispatch, tab traversal order/shift reversal, scope-bounded traversal boundaries, directional traversal fallback, and geometry-aware directional candidate selection via traversal bounds. | Transform-aware directional policies and deeper scope hierarchy edge cases. |
| Editable text input | `src/Flutter.Tests/TextInputTests.cs` | Focused text input delivery via host -> focus manager dispatch, controller text mutation, selection-aware insert/delete behavior, controller composing lifecycle (`SetComposing`/`CommitComposing`/`ClearComposing`), focus-manager composition dispatch (`update`/`commit`), caret/selection navigation keys (`Left/Right/Home/End`, shift extend), `Ctrl/Meta+A` selection, disabled-input behavior, and `onChanged` callback flow. | Host-native IME preedit bridge, caret geometry rendering, multiline editing behavior, and clipboard/shortcut expansion beyond select-all. |
| Scroll/slivers core | `src/Flutter.Tests/ScrollPipelineTests.cs` | Scroll position physics, viewport/sliver layout, cache extent, keep-alive reuse, sliver padding offset behavior, and `Scrollable + ListView.Separated` viewport continuity under jump-driven scroll updates. | Stress tests with very large child counts and rapid direction changes. |
| Scroll widget infrastructure | `src/Flutter.Tests/ScrollInfrastructureTests.cs` | Notifications, primary controller, keep-alive mixin, list/grid constructor semantics. | Combined nested scrollables and scrollbar interaction nuances. |
| Sample state behavior | `src/Flutter.Tests/SampleCounterStateTests.cs` | Counter model notifications and scope dependency behavior. | End-to-end sample page regressions across all demo routes. |

## How to Use for Iterative Work

1. Pick feature area row.
2. Read listed tests before opening implementation files.
3. Update or add tests in same row when behavior changes.
4. If no row fits, add one before shipping the feature.
