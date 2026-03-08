# Test Matrix

Purpose: map framework areas to existing test coverage and identify common gaps quickly.

## Coverage Map

| Area | Primary tests | What is covered | Typical gap to watch |
| --- | --- | --- | --- |
| Element lifecycle and reconciliation | `src/Flutter.Tests/ElementLifecycleTests.cs` | Keyed reorder, global key reparent/activation/disposal behavior. | Complex multi-branch updates with mixed keyed/unkeyed subtrees. |
| Inherited dependencies | `src/Flutter.Tests/InheritedWidgetTests.cs`, `src/Flutter.Tests/InheritedModelTests.cs`, `src/Flutter.Tests/InheritedNotifierTests.cs` | Dependent registration and selective rebuild semantics. | Edge cases around deep tree shadowing plus dynamic notifier swaps. |
| Frame and scheduler flow | `src/Flutter.Tests/FramePipelineTests.cs` | Begin/draw/post-frame order, persistent callbacks, build scheduling in frame. | Long-running callback interactions with host-level visibility transitions. |
| Render object parity | `src/Flutter.Tests/RenderingParityTests.cs` | Constraint normalization, relayout boundary behavior, root relayout triggers. | Rare constraint edge cases across proxy chains. |
| Compositing and layers | `src/Flutter.Tests/CompositingLayerTests.cs`, `src/Flutter.Tests/LayerV2Tests.cs` | Repaint boundary behavior, layer reuse/property updates, layer tree structure. | Layer churn under repeated boundary toggles in large trees. |
| Semantics tree | `src/Flutter.Tests/SemanticsTreeTests.cs` | Actions, merge/split, transform/clip, dirty propagation and id reuse contracts. | Host accessibility bridge integration and end-to-end assistive flows. |
| Gestures and hit testing | `src/Flutter.Tests/GesturePipelineTests.cs` | Transform/clip hit testing, recognizer dispatch, arena conflict outcomes, pointer signals. | Multi-pointer gesture interactions and cancellation races. |
| Navigation | `src/Flutter.Tests/NavigationTests.cs` | Push/pop/replace/remove APIs, named routes, observers, route data semantics. | Host back integration across nested navigators. |
| Scroll/slivers core | `src/Flutter.Tests/ScrollPipelineTests.cs` | Scroll position physics, viewport/sliver layout, cache extent, keep-alive reuse. | Stress tests with very large child counts and rapid direction changes. |
| Scroll widget infrastructure | `src/Flutter.Tests/ScrollInfrastructureTests.cs` | Notifications, primary controller, keep-alive mixin, list/grid constructor semantics. | Combined nested scrollables and scrollbar interaction nuances. |
| Sample state behavior | `src/Flutter.Tests/SampleCounterStateTests.cs` | Counter model notifications and scope dependency behavior. | End-to-end sample page regressions across all demo routes. |

## How to Use for Iterative Work

1. Pick feature area row.
2. Read listed tests before opening implementation files.
3. Update or add tests in same row when behavior changes.
4. If no row fits, add one before shipping the feature.
