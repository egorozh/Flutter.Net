# Framework Master Plan

This is the single source of truth for framework status, direction, and implementation priorities.

## AI Semantic Snapshot

Use this block as the fastest machine-readable status summary.

```yaml
framework_plan_version: 1
last_updated: 2026-03-12
north_star: "Flutter-like widget/rendering framework in C# with Avalonia as host infrastructure."
current_phase: "M4 material library rewrite (theme/scaffold/material controls) in progress."
status:
  widget_element_state_lifecycle: done
  render_pipeline_layout_paint_compositing_semantics: done
  scheduler_ticker_frame_flow: done
  gesture_arena_and_recognizers: done
  navigation_stack_and_observers: done
  scroll_sliver_list_grid_pipeline: done
  desktop_widget_host_app_flow: done
  material_library_rewrite: in_progress
  browser_android_ios_sample_hosts: planned
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
    status: done
  - id: M4
    title: "Material library rewrite"
    status: in_progress
  - id: M5
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

Progress update (2026-03-11):

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

Status: `done`

Completion note:

- Closed on 2026-03-11 after delivering the planned port-first widget baseline set (proxy, alignment, stack/positioned, decoration/container composition, ratio/fractional/fitted sizing, unconstrained/overflow/offstage) with mirrored C#/Dart sample routes and focused regression coverage.
- Included post-baseline text-rendering parity hardening needed for control-port fidelity: `Text` now wires `textAlign`/`softWrap`/`maxLines`/`overflow`/`textDirection`, and `RenderParagraph` no longer applies a synthetic `maxWidth=1000` cap for unbounded layout.
- Continued post-M3 typography parity hardening: framework `Text` now exposes `fontWeight`, `fontStyle`, `height`, and `letterSpacing`, with matching `RenderParagraph` layout support and host-default font-family defaults across paragraph/button/editable text layout paths.
- Continued text-style inheritance parity hardening: framework now includes `DefaultTextStyle`/`TextStyle`, and `Text` resolves inherited typography defaults (`fontFamily`, `fontSize`, `color`, `fontWeight`, `fontStyle`, `height`, `letterSpacing`) with local override precedence; sample root now provides a Material-like default body style to reduce C#/Dart menu text wrapping and line-height drift.
- Continued paragraph-alignment parity hardening: `RenderParagraph` now normalizes loose-width `center/right/end` layout width to content width when host layout reports positive internal glyph offset, eliminating right-shifted intrinsic label paint in sample list/button scenarios while retaining tight-width aligned behavior.
- Counter sample viewport hardening: `CounterScreen` now uses outer `SingleChildScrollView` in both C# and Dart samples so smaller desktop heights avoid `RenderFlex` bottom-overflow debug zones while preserving existing demo modules on the page.
- Overflow-debug parity progression: `RenderFlex` now paints Flutter-style yellow/black overflow indicators with clipped overflow child paint, 45-degree marker geometry, and edge-aligned/rotated labels for main-axis overflow; both samples include a dedicated overflow-indicator demo page for runtime verification.

Completion snapshot:

- Added first proxy-widget port baseline in framework widget layer: `Opacity`, `Transform`, and `ClipRect` wrappers over existing render primitives (`RenderOpacity`, `RenderTransform`, `RenderClipRect`) with focused rebuild/update regression coverage.
- Added sample parity route/page in both C# and Dart sample galleries for interactive proxy-widget composition checks (`Opacity`, `Transform`, `ClipRect`).
- Fixed compositing edge case where repaint-boundary layer-property updates could be dropped when repaint and composited-layer invalidation happened in the same frame.
- Added single-child alignment baseline in framework widget layer: `Align` and `Center` over new `RenderAlign`, including width/height shrink factors and parity sample route/page in both C# and Dart galleries.
- Added multi-child overlay baseline in framework widget layer: `Stack` and `Positioned` over new `RenderStack`/`StackParentData`, including positioned insets/size behavior and parity sample route/page in both C# and Dart galleries.
- Added decoration baseline in framework widget layer: `DecoratedBox` over `RenderDecoratedBox` plus value objects (`BoxDecoration`, `BorderSide`, `BorderRadius`) and parity sample route/page in both C# and Dart galleries.
- Extended `Container` composition baseline with `alignment`, `margin`, `constraints`, and `transform` support (including Flutter-like width/height tightening against explicit constraints), plus parity sample route/page updates in both C# and Dart galleries.
- Added ratio/flex layout primitive baseline in framework widget layer: `AspectRatio` over new `RenderAspectRatio` plus `Spacer` (expanded tight-flex gap helper), with regression coverage for ratio sizing, widget update wiring, and flex parent-data propagation.
- Added sample parity route/page in both C# and Dart sample galleries for interactive `AspectRatio` and `Spacer` behavior checks.
- Hardened `Spacer` demo observability and coverage: updated parity demo to compare two spacer slots with asymmetric flex and added regression coverage for proportional `RenderFlex` distribution across two spacer allocations.
- Added fractional sizing baseline in framework widget layer: `FractionallySizedBox` over new `RenderFractionallySizedBox` with bounded-axis factor constraints and alignment-aware child placement, plus parity sample route/page in both C# and Dart galleries.
- Added fitted scaling baseline in framework widget layer: `FittedBox` over new `RenderFittedBox` with `BoxFit` sizing semantics, transform-aware paint/hit-test mapping, and alignment-controlled placement.
- Added sample parity route/page in both C# and Dart sample galleries for interactive `FittedBox` (`contain/cover/fill/none/scaleDown`) and alignment checks.
- Added unconstrained/limited constraints baseline in framework widget layer: `UnconstrainedBox` over `RenderUnconstrainedBox` (axis-specific unconstraining + alignment) and `LimitedBox` over `RenderLimitedBox` (max clamp applied only on unbounded axes).
- Added sample parity route/page in both C# and Dart sample galleries for interactive `UnconstrainedBox + LimitedBox` behavior checks (`constrainedAxis` and `maxWidth/maxHeight`).
- Added overflow constraints baseline in framework widget layer: `OverflowBox` over `RenderConstrainedOverflowBox` (optional min/max overrides + `OverflowBoxFit` sizing mode) and `SizedOverflowBox` over `RenderSizedOverflowBox` (fixed own size with parent constraints passed through to child).
- Added sample parity route/page in both C# and Dart sample galleries for interactive `OverflowBox + SizedOverflowBox` behavior checks (`fit`, `alignment`, override max constraints, requested own size).
- Added offstage layout baseline in framework widget layer: `Offstage` over `RenderOffstage` with Flutter-like offstage behavior (child still laid out, parent collapses to smallest size, and paint/hit-test/semantics participation suppressed while offstage).
- Added sample parity route/page in both C# and Dart sample galleries for interactive `Offstage` behavior checks in row layout (state toggle and zero-space collapse).

Exit criteria:

- Priority controls needed for Dart-to-C# rewrites are identified and implemented in framework layers.
- New widgets use the same architecture boundaries (`Widget -> Element -> RenderObject`) without leaking behavior into Avalonia controls.
- Sample gallery includes representative real-world compositions beyond demos.

### M4. Material Library Rewrite

Status: `in_progress`

Kickoff note (2026-03-12):

- Prioritized immediately after M3 to unblock practical control rewrites and reduce sample-level styling drift by introducing a Flutter-like Material layer in framework widgets.

Progress update (2026-03-12):

- Added dedicated framework Material assembly: `src/Flutter.Material/Flutter.Material.csproj`.
- Introduced initial theming primitives: `ThemeData`, `MaterialTextTheme`, and inherited `Theme`.
- `Theme` now propagates baseline `TextTheme.BodyMedium` through `DefaultTextStyle`, enabling framework `Text` defaults without sample-only wrappers.
- C# sample app root now uses `Theme(data: ThemeData.Light, child: ...)`; Dart sample root now sets explicit `MaterialApp` text-theme baseline (`bodyMedium` 14/1.43/0.25) for parity.
- Added regression coverage for theme-to-text propagation in `src/Flutter.Tests/TextWidgetTests.cs`.
- Added Material shell primitives: `Scaffold` and `AppBar` in `src/Flutter.Material` with baseline slot wiring (`body`, `appBar`, `floatingActionButton`, `bottomNavigationBar`, title/leading/actions).
- C# sample gallery pages now use framework `Scaffold`/`AppBar` composition for menu/demo shells; Dart sample gallery mirrors the same structural shell usage.
- Added regression coverage for scaffold/app-bar theme resolution and widget composition behavior in `src/Flutter.Tests/MaterialScaffoldTests.cs`.
- Added first Material control set: `TextButton`, `ElevatedButton`, and `OutlinedButton` in `src/Flutter.Material` with inherited-theme defaults and disabled-state styling behavior.
- Added Material buttons demo route/page in both C# and Dart sample galleries for parity/runtime validation.
- Added regression coverage for Material button default color resolution and disabled visuals in `src/Flutter.Tests/MaterialButtonsTests.cs`.
- Added initial Material button interaction polish: pointer-pressed visuals, focus visuals, and keyboard activation (`Enter`/`Return`/`Space`) through `Focus` integration in `MaterialButtonCore`.
- Sample gallery shell buttons (menu entries and demo-page back action) now use Material button controls on both C# and Dart samples; Material-buttons demo control-strip actions now also use Material buttons instead of `CounterTapButton`.
- Added core framework support for stateful widgets implemented in external assemblies (`State.StateWidget` protected accessor) to keep `src/Flutter.Material` decoupled while preserving stateful widget patterns.
- Applied strict parity follow-up for button defaults/state layers from Flutter Dart source: `TextButton`/`ElevatedButton`/`OutlinedButton` now enforce baseline minimum size `64x40`, use stadium-like default radius, and use normalized state-layer overlay (`pressed/focused`) instead of custom focus-border widening heuristics.
- Continued strict parity follow-up for button theming tokens/defaults: `ThemeData` now exposes `onSurfaceColor`, `outlineColor`, and `surfaceContainerLowColor`; `ElevatedButton` defaults now use surface-container/primary color pairing with on-surface disabled tones; `OutlinedButton` default border now resolves from outline token while foreground remains primary.
- Added hover interaction baseline for Material buttons: framework pointer stack now dispatches enter/exit transitions (`PointerEnterEvent`/`PointerExitEvent` via `GestureBinding` hover-hit tracking), and `MaterialButtonCore` applies hover state-layer opacity (`0.08`) in addition to pressed/focused overlays.
- Fixed pointer-focus visual parity for Material buttons: pointer clicks no longer leave persistent focus tint after release (`PointerUp`), while keyboard activation still enables focus overlay behavior.
- Improved Material ripple visibility parity on wider buttons by delaying splash alpha fade until the tail phase of expansion, plus added regression coverage that `RenderInkSplash` matches full tight button bounds.
- Fixed clip-layer resize invalidation for rounded/rect clips used by button ripple paths (`RenderClipRRect`/`RenderClipRect`): implicit size-based clip bounds now refresh on layout size changes, preventing stale ripple zones after viewport resize.
- Added ink/ripple baseline for Material buttons with rounded clipping parity: framework now includes animated radial splash paint support (`RenderInkSplash` + `InkSplash`), rounded clip primitives (`ClipRRect` widget/render/layer + `PaintingContext.PushClipRRect`), and `MaterialButtonCore` triggers splash animation from pointer origin (keyboard fallback: center origin) while clipping splash by button border radius.

Initial scope:

- Introduce framework-level theming primitives (`ThemeData`, `Theme`, baseline color/text style propagation).
- Introduce shell/layout primitives for Material app structure (`Scaffold`, `AppBar`, and supporting slots).
- Introduce first Material control set (`TextButton`, `ElevatedButton`, `OutlinedButton`) on top of framework render/widget layers.
- Keep architecture boundaries explicit: behavior in framework libraries (`src/Flutter`, `src/Flutter.Material`), host integration in sample hosts only.

Exit criteria:

- Material theming is available through inherited framework state and can drive common control defaults.
- Material shell primitives are sufficient to host route pages without custom sample-only wrappers.
- Initial Material control set supports core states and API shape needed for straightforward Dart-to-C# rewrites.
- Regression coverage exists for widget-to-render wiring and theming resolution behavior.

### M5. Cross-Host Sample Parity and Stability

Status: `planned`

Scheduling note (2026-03-12):

- Moved after Material rewrite as a final stabilization milestone. Current blockers are local toolchain/environment alignment (Android API 36 SDK platform missing; iOS workload/Xcode version mismatch).

Exit criteria:

- Desktop, browser, Android, and iOS sample hosts build successfully from the solution.
- Framework-driven app flow remains identical across hosts.
- `src/Sample/Flutter.Net` and `dart_sample` stay in feature/route/module parity.

## Backlog Candidates (After M1-M5)

- Text editing/IME primitives and richer text input workflows.
- Overlay/portal-like primitives and advanced route transitions.
- Performance instrumentation and frame diagnostics tooling.
- Expanded documentation for migration recipes from Flutter (Dart) widgets to C#.

## Update Protocol (For Humans and AI Agents)

- Always update this file when milestone status changes (`done`, `in_progress`, `planned`, `blocked`).
- Always record shipped outcomes in `CHANGELOG.md`.
- For Dart-to-C# control/widget ports, follow mandatory parity-first workflow in `docs/ai/PORTING_MODE.md` (strict `1:1` default; documented divergences only).
- For every meaningful feature change, update both:
  - semantic status (this document),
  - historical record (`CHANGELOG.md`).
- Keep architecture boundaries explicit: framework behavior in framework libraries (`src/Flutter`, `src/Flutter.Material`), host adaptation only in sample hosts.
