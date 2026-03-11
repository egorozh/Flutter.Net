# Framework Master Plan

This is the single source of truth for framework status, direction, and implementation priorities.

## AI Semantic Snapshot

Use this block as the fastest machine-readable status summary.

```yaml
framework_plan_version: 1
last_updated: 2026-03-11
north_star: "Flutter-like widget/rendering framework in C# with Avalonia as host infrastructure."
current_phase: "Port-first widget set expansion (M3) in progress."
status:
  widget_element_state_lifecycle: done
  render_pipeline_layout_paint_compositing_semantics: done
  scheduler_ticker_frame_flow: done
  gesture_arena_and_recognizers: done
  navigation_stack_and_observers: done
  scroll_sliver_list_grid_pipeline: done
  desktop_widget_host_app_flow: done
  browser_android_ios_sample_hosts: in_progress
  dart_to_csharp_control_porting_readiness: in_progress
  docs_alignment_and_tracking: in_progress
next_milestones:
  - id: M1
    title: "Core parity hardening"
    status: done
  - id: M2
    title: "Input/focus/accessibility completion"
    status: done
  - id: M3
    title: "Port-first widget set expansion"
    status: in_progress
  - id: M4
    title: "Cross-host sample parity and stability"
    status: planned
```

## Confirmed Done (Repository Baseline)

- [x] Flutter-like core abstractions exist and are wired: `Widget -> Element -> RenderObject`.
- [x] Stateful lifecycle and build scheduling are implemented (`State`, `SetState`, `BuildOwner`).
- [x] Inherited dependency model is implemented (`InheritedWidget`, `InheritedModel`, `InheritedNotifier`).
- [x] Render pipeline is implemented (`PipelineOwner`, layout/compositing/paint/semantics phases).
- [x] Layer tree primitives are implemented (offset/opacity/transform/clip/picture layers).
- [x] Gesture system is implemented (pointer router, arena, tap/drag/long-press recognizers).
- [x] Navigation stack is implemented (`Navigator`, routes, named routes, observers, back handling).
- [x] Scroll/sliver stack is implemented (`Scrollable`, `Viewport`, sliver lists/grids, keep-alive, notifications).
- [x] Widget host path is active on desktop (`FlutterExtensions.Run` + `WidgetHost`).
- [x] Sample gallery demonstrates navigation, scrolling, and editable text/focus demos through framework widgets.
- [x] Automated test project exists and covers lifecycle, rendering, layers, semantics, gestures, navigation, and scrolling.

## Global Plan

### M1. Core Parity Hardening

Status: `done`

Completion note:

- Closed on 2026-03-10 after targeted parity hardening passes for layout exception surfacing, constraint clamping behavior, and render/compositing invalidation no-op guards.

Exit criteria:

- Core render object semantics remain compatible with expected Flutter behavior in covered scenarios.
- Existing test suites stay green while adding missing parity edge-cases.
- Layout/paint/semantics invalidation rules are documented and predictable for contributors.

### M2. Input, Focus, and Accessibility Completion

Status: `done`

Completion note:

- Closed on 2026-03-10 after delivering text-editing ergonomics baselines (word/paragraph shortcuts, clipboard copy/cut/paste, grapheme-safe caret/delete behavior), transform-aware directional traversal rect resolution, and host semantics bridge runtime surface (`SemanticsRoot`, `SemanticsUpdated`, action dispatch) with coverage in framework tests.

Progress update (2026-03-10):

- Keyboard/focus baseline is implemented and host-wired (`KeyEvent`, `FocusNode`, `FocusManager`, `Focus`).
- Focus scopes are now available (`FocusScopeNode`, `FocusScope`) and traversal is bounded to the active scope (Tab + directional keys).
- Directional traversal includes a geometry-aware policy when focus bounds are available, with deterministic sequential fallback.
- Directional traversal now resolves traversal geometry through attached render-object transforms (including `RenderTransform`) before directional candidate ranking.
- Editable text baseline is integrated (`EditableText`, `TextEditingController`, host text input dispatch into focused node callbacks).
- Editable controller/selection baseline is integrated (`TextSelection`, `TextRange`, selection-aware insert/delete/navigation, `Ctrl/Meta+A`).
- Editable composition lifecycle baseline is integrated (focus-manager composition update/commit dispatch, controller composing state, editable-widget composition handling).
- Host-native IME preedit bridge baseline is integrated (`TextInputMethodClientRequested` -> `TextInputMethodClient.SetPreeditText` -> focus-manager composition updates).
- IME state sync baseline is integrated (`surrounding text`, selection sync, and cursor rectangle exposure from focused editable state through host text-input client).
- Multiline editing + glyph-aware caret baseline is integrated (multiline mode with `Enter` newline insertion, `ArrowUp/ArrowDown` vertical navigation, and `TextLayout`-driven caret rectangle with host-less fallback).
- Word-level text-editing shortcuts baseline is integrated (`Ctrl/Alt + ArrowLeft/ArrowRight` word navigation and `Ctrl/Alt + Backspace/Delete` word deletion in `EditableText`).
- Paragraph-level caret shortcuts baseline is integrated for multiline editing (`Ctrl/Alt + ArrowUp/ArrowDown` paragraph start/end navigation with selection extension support).
- Clipboard/action shortcut baseline is integrated (`Ctrl/Meta + C/X/V`) with framework clipboard cache and host clipboard synchronization hooks in `FlutterHost`.
- Grapheme-aware editing baseline is integrated (caret and delete operations move by text elements instead of UTF-16 code units).
- Host accessibility bridge documentation baseline is now captured (host-side semantics tree consumption and action dispatch expectations per target host).
- Host accessibility bridge runtime baseline is integrated in `FlutterHost` (semantics root exposure, semantics-updated event, and action routing API), with regression coverage.

Exit criteria:

- Keyboard/focus flow is implemented as first-class framework behavior.
- Semantics actions and tree updates are stable for interactive controls.
- Platform accessibility bridge expectations are documented per host.

### M3. Port-First Widget Set Expansion

Status: `in_progress`

Progress update (2026-03-10):

- Added first proxy-widget port baseline in framework widget layer: `Opacity`, `Transform`, and `ClipRect` wrappers over existing render primitives (`RenderOpacity`, `RenderTransform`, `RenderClipRect`) with focused rebuild/update regression coverage.
- Added sample parity route/page in both C# and Dart sample galleries for interactive proxy-widget composition checks (`Opacity`, `Transform`, `ClipRect`).
- Fixed compositing edge case where repaint-boundary layer-property updates could be dropped when repaint and composited-layer invalidation happened in the same frame.
- Added single-child alignment baseline in framework widget layer: `Align` and `Center` over new `RenderAlign`, including width/height shrink factors and parity sample route/page in both C# and Dart galleries.
- Added multi-child overlay baseline in framework widget layer: `Stack` and `Positioned` over new `RenderStack`/`StackParentData`, including positioned insets/size behavior and parity sample route/page in both C# and Dart galleries.
- Added decoration baseline in framework widget layer: `DecoratedBox` over `RenderDecoratedBox` plus value objects (`BoxDecoration`, `BorderSide`, `BorderRadius`) and parity sample route/page in both C# and Dart galleries.
- Extended `Container` composition baseline with `alignment`, `margin`, `constraints`, and `transform` support (including Flutter-like width/height tightening against explicit constraints), plus parity sample route/page updates in both C# and Dart galleries.
- Added ratio/flex layout primitive baseline in framework widget layer: `AspectRatio` over new `RenderAspectRatio` plus `Spacer` (expanded tight-flex gap helper), with regression coverage for ratio sizing, widget update wiring, and flex parent-data propagation.
- Added sample parity route/page in both C# and Dart sample galleries for interactive `AspectRatio` and `Spacer` behavior checks.

Exit criteria:

- Priority controls needed for Dart-to-C# rewrites are identified and implemented in framework layers.
- New widgets use the same architecture boundaries (`Widget -> Element -> RenderObject`) without leaking behavior into Avalonia controls.
- Sample gallery includes representative real-world compositions beyond demos.

### M4. Cross-Host Sample Parity and Stability

Status: `planned`

Exit criteria:

- Desktop, browser, Android, and iOS sample hosts build successfully from the solution.
- Framework-driven app flow remains identical across hosts.
- `src/Sample/Flutter.Net` and `dart_sample` stay in feature/route/module parity.

## Backlog Candidates (After M1-M4)

- Text editing/IME primitives and richer text input workflows.
- Overlay/portal-like primitives and advanced route transitions.
- Performance instrumentation and frame diagnostics tooling.
- Expanded documentation for migration recipes from Flutter (Dart) widgets to C#.

## Update Protocol (For Humans and AI Agents)

- Always update this file when milestone status changes (`done`, `in_progress`, `planned`, `blocked`).
- Always record shipped outcomes in `CHANGELOG.md`.
- For every meaningful feature change, update both:
  - semantic status (this document),
  - historical record (`CHANGELOG.md`).
- Keep architecture boundaries explicit: framework behavior in `src/Flutter`, host adaptation only in sample hosts.
