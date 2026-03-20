# Invariants

These rules are non-negotiable unless explicitly changed via documented architecture decision.

## Architecture Boundaries

- Framework behavior must stay in framework libraries under `src/` (`src/Flutter`, `src/Flutter.Material`).
- Avalonia is host/platform infrastructure, not business logic for framework widgets.
- Core direction remains `Widget -> Element -> RenderObject -> platform adapter`.

## Dart Porting Invariants

- Dart implementation is the source of truth for matching controls/widgets; ports must follow strict `1:1` structure/behavior by default (see `docs/ai/PORTING_MODE.md`).
- Missing primitives must be implemented in framework layers first; do not hide parity gaps with control-local workarounds.
- Any intentional divergence from Dart behavior must be documented with reason and expected delta in the same iteration.

## Widget and Element Lifecycle

- `StatefulWidget` identity is preserved only when reconciliation keys/type allow it.
- `GlobalKey` reparenting must not dispose state when reinserted in same frame lifecycle.
- `BuildOwner` is the owner of dirty build scheduling; build work runs in frame flow.
- Inherited dependencies must notify only registered dependents per contract (`InheritedWidget/Model/Notifier`).

## Rendering Pipeline

- Pipeline phase order is stable: layout -> compositing bits -> paint -> semantics.
- Layout must not run with non-normalized constraints.
- Repaint boundaries own isolated layers and avoid unnecessary child repaint.
- Semantics updates are part of pipeline flush and must reflect render tree state.

## Input, Gestures, and Hit Testing

- Pointer events are routed through `GestureBinding` and gesture arena resolution.
- Hit testing must apply transform/clip semantics before dispatch.
- Recognizer conflict resolution should remain deterministic for covered scenarios.

## Navigation

- `Navigator` route stack must always keep a valid top route.
- Observer callbacks (`didPush/didPop/didReplace/didRemove`) must match stack mutations.
- Back-button handling should route through navigator APIs, not host-only ad hoc logic.

## Scroll and Slivers

- `ScrollPosition` and physics must clamp/advance within computed scroll extents.
- Viewport/sliver contracts define child creation, eviction, keep-alive reuse, and cache behavior.
- High-level widgets (`ListView`, `GridView`, `Scrollbar`) should map to sliver pipeline primitives.

## Sample Parity

- Feature/route/module parity between `src/Sample/Flutter.Net` and `dart_sample` is required for sample-level changes.

## Fast Safety Checks

- Lifecycle: `src/Flutter.Tests/ElementLifecycleTests.cs`
- Inherited: `src/Flutter.Tests/InheritedWidgetTests.cs`, `src/Flutter.Tests/InheritedModelTests.cs`, `src/Flutter.Tests/InheritedNotifierTests.cs`
- Pipeline: `src/Flutter.Tests/FramePipelineTests.cs`, `src/Flutter.Tests/RenderingParityTests.cs`
- Layers: `src/Flutter.Tests/CompositingLayerTests.cs`, `src/Flutter.Tests/LayerV2Tests.cs`
- Gestures: `src/Flutter.Tests/GesturePipelineTests.cs`
- Navigation: `src/Flutter.Tests/NavigationTests.cs`
- Scroll: `src/Flutter.Tests/ScrollPipelineTests.cs`, `src/Flutter.Tests/ScrollInfrastructureTests.cs`
- Semantics: `src/Flutter.Tests/SemanticsTreeTests.cs`
