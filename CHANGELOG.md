# Changelog

All notable framework changes are documented in this file.

This project follows the spirit of [Keep a Changelog](https://keepachangelog.com/en/1.1.0/).

## [Unreleased]

### Added

- Lifecycle parity hardening: added element reconciliation tests for mixed keyed/unkeyed updates, including nested multi-parent reorder scenarios, to verify keyed state retention, stable-tail reuse, and disposal of moved unkeyed states (`src/Flutter.Tests/ElementLifecycleTests.cs`).
- Scroll parity hardening: fixed `RenderSliverPadding` child paint offset to avoid double-applying scroll offset (preventing viewport gaps when padded slivers are scrolled), and added regression coverage for `Scrollable` + `ListView.Separated` viewport continuity during controller jumps (`src/Flutter/Rendering/Sliver.cs`, `src/Flutter.Tests/ScrollPipelineTests.cs`).
- Rendering parity hardening: `RenderObject.Layout` no longer swallows layout exceptions, with regression coverage that verifies exception propagation and dirty-state preservation after failed layout; added text-layout fallback sizing for host-less/font-manager-less environments used by framework tests (`src/Flutter/Rendering/Object.RenderObject.cs`, `src/Flutter.Tests/RenderingParityTests.cs`, `src/Flutter/RenderParagraph.cs`, `src/Flutter/RenderButton.cs`, `src/Flutter/UI/TextLayoutFallback.cs`).
- Constraints parity hardening: `BoxConstraints.Tighten(width/height)` now clamps requested values to the existing min/max range, with regression coverage for out-of-range tighten requests (`src/Flutter/Rendering/Box.cs`, `src/Flutter.Tests/RenderingParityTests.cs`).
- Dart source traceability: annotated all solution-tracked C# files with `Dart parity source (reference)` comments to speed up Dart-to-C# parity review and future porting iterations.

### Planned

- Continue Flutter parity for widgets, layout, rendering, gestures, semantics, and navigation.
- Expand host/runtime validation for desktop, browser, and mobile.
- Improve architecture docs and migration guidance for Dart-to-C# rewrites.

## [2026-03-08] - Baseline framework snapshot

### Added

- Flutter-like widget framework core:
  - `Widget`, `Element`, `BuildContext`, `State`, `StatelessWidget`, `StatefulWidget`.
  - `BuildOwner` scheduling and rebuild flow with dirty element processing.
  - Inherited primitives: `InheritedWidget`, `InheritedModel`, `InheritedNotifier`.
- Frame and render pipeline:
  - `Scheduler` with begin/draw/post-frame phases and persistent callbacks.
  - `PipelineOwner` with layout, compositing, paint, and semantics flushing.
  - `RenderObject`/`RenderBox` tree with hit testing and parent/child contracts.
  - Layer tree primitives (`OffsetLayer`, `OpacityLayer`, `TransformLayer`, `ClipRectLayer`, `PictureLayer`).
- Framework host integration:
  - `FlutterHost` + `WidgetHost` to mount widget trees into Avalonia host controls.
  - Pointer/key event bridging to framework gesture and navigation layers.
- Widgets and rendering primitives:
  - Basic widgets (`Container`, `Padding`, `SizedBox`, `ColoredBox`, `Row`, `Column`, `Flexible`, `Expanded`, `Text`, `Button`).
  - Proxy render objects (`RenderPadding`, `RenderColoredBox`, `RenderOpacity`, `RenderTransform`, `RenderClipRect`, pointer listener).
  - Flex/layout render implementation and text/button render objects.
- Gestures and input pipeline:
  - Pointer routing, gesture arena, and recognizers (tap, drag, long press).
  - Gesture detector and raw gesture widget wiring.
- Navigation stack:
  - Route model (`Route`, `ModalRoute`, `PageRoute`, builder route).
  - `Navigator` APIs for push/pop/replace/named routes and observer hooks.
  - Route observer support and back-button handling integration.
- Scroll/sliver infrastructure:
  - `ScrollController`, `Scrollable`, `Viewport`, `CustomScrollView`, `SingleChildScrollView`.
  - Sliver adapters and lists/grids (`SliverList`, `SliverGrid`, `SliverFixedExtentList`, padding, delegates).
  - High-level widgets (`ListView`, `GridView`, `Scrollbar`), keep-alive and notifications.
- Semantics:
  - Semantics configuration and node ownership model.
  - Semantics updates integrated into pipeline flush cycle.
- Sample app and host targets:
  - Sample gallery with counter, navigator, list/grid/sliver/scrollbar demos.
  - Host entry points for desktop, browser, Android, and iOS sample projects.
- Test coverage foundation (`src/Flutter.Tests`):
  - Lifecycle/reconciliation, inherited dependencies, frame pipeline, rendering parity.
  - Compositing layers, semantics tree behavior, gesture pipeline, scroll pipeline/infrastructure.
  - Navigation stack and observer semantics.

### Notes

- Current strategic direction and open milestones are tracked in `docs/FRAMEWORK_PLAN.md`.
