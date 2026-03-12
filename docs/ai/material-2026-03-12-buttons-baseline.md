# Feature: material-2026-03-12-buttons-baseline

## Goal

- Deliver M4 first Material control set by adding `TextButton`, `ElevatedButton`, and `OutlinedButton` to `Flutter.Material` with theme defaults and disabled styling behavior.

## Non-Goals

- No full Material button-style system (`ButtonStyle` parity, state-property resolution, animation layer) in this iteration.
- No full advanced interaction parity (hover tracking, ink ripple/elevation animation, state-property matrix).
- No cross-host environment/toolchain fixes for Android/iOS blockers.

## Context Budget Plan

- Budget: max 8 files in initial read.
- Entry files:
  - `docs/FRAMEWORK_PLAN.md`
  - `docs/ai/MODULE_INDEX.md`
  - `src/Flutter.Material/ThemeData.cs`
  - `src/Flutter.Material/Scaffold.cs`
  - `src/Sample/Flutter.Net/SampleGalleryScreen.cs`
  - `dart_sample/lib/sample_gallery_screen.dart`
  - `src/Flutter/Widgets/Gestures.cs`
  - `src/Flutter.Tests/MaterialScaffoldTests.cs`
- Expansion trigger:
  - Expand only when button composition/default-color assertions required additional render/widget test references.

## Invariants Impacted

- [x] `docs/ai/INVARIANTS.md` reviewed
- List invariants that this feature touches:
  - Framework behavior remains in framework libraries (`src/Flutter`, `src/Flutter.Material`), not host controls.
  - Material controls remain implemented via framework widget composition over existing render primitives.
  - Sample C#/Dart route/module parity remains synchronized for demo-route additions.

## Planned Changes

- Files to edit:
  - `src/Flutter.Material/Buttons.cs`
  - `src/Flutter.Material/ThemeData.cs`
  - `src/Sample/Flutter.Net/MaterialButtonsDemoPage.cs`
  - `src/Sample/Flutter.Net/SampleGalleryScreen.cs`
  - `dart_sample/lib/material_buttons_demo_page.dart`
  - `dart_sample/lib/sample_gallery_screen.dart`
  - `dart_sample/lib/sample_routes.dart`
  - `src/Flutter.Tests/MaterialButtonsTests.cs`
  - `CHANGELOG.md`
  - `docs/FRAMEWORK_PLAN.md`
  - `docs/ai/MODULE_INDEX.md`
  - `docs/ai/PARITY_MATRIX.md`
  - `docs/ai/TEST_MATRIX.md`
- Brief intent per file:
  - `Buttons.cs`: implement `TextButton`/`ElevatedButton`/`OutlinedButton` with theme defaults and disabled visuals.
  - sample files: add Material buttons demo route/page parity in both C# and Dart galleries.
  - `MaterialButtonsTests.cs`: lock button default-theme and disabled behavior in regression tests.
  - docs/changelog: record shipped M4 control-set progress and coverage updates.

## Follow-up Iteration (2026-03-12)

- Extended `MaterialButtonCore` from stateless to stateful composition with:
  - pointer-pressed visual state,
  - focus visual treatment,
  - keyboard activation for `Enter`/`Return`/`Space`.
- Fixed button sizing in unbounded vertical layouts by replacing internal `Center` with shrink-wrapped `Align(widthFactor: 1, heightFactor: 1)`.
- Applied strict parity follow-up from Flutter button sources (`text_button.dart`, `elevated_button.dart`, `outlined_button.dart`):
  - baseline minimum size aligned to `64x40`,
  - default radius moved to stadium-like shape approximation,
  - state feedback aligned to overlay layer model (`pressed/focused` 0.10) instead of custom focus-border thickening.
- Added regression coverage for baseline minimum size (`TextButton` default `64x40`) in `MaterialButtonsTests`.
- Continued strict parity defaults alignment with explicit theme tokens:
  - `ThemeData` expanded with `OnSurfaceColor`, `OutlineColor`, `SurfaceContainerLowColor`,
  - `ElevatedButton` defaults now resolve `foreground/background/disabled` from `primary/surfaceContainerLow/onSurface`,
  - `OutlinedButton` defaults now resolve border from `outline` and disabled border/foreground from `onSurface`.
- Added regression coverage for the new default mapping:
  - `ElevatedButton_UsesThemeSurfaceContainerAndPrimaryColorsByDefault`,
  - `OutlinedButton_UsesThemeOutlineColorForBorderByDefault`,
  - `OutlinedButton_DefaultForegroundUsesThemePrimaryColor`,
  - `ElevatedButton_DisabledStateUsesThemeOnSurfaceTones`.
- Added hover-state parity baseline:
  - framework pointer layer now dispatches `PointerEnterEvent`/`PointerExitEvent` via hover hit-path transitions,
  - `MaterialButtonCore` now resolves hover overlay with `0.08` opacity and clears it on exit,
  - regression coverage added in `TextButton_HoverStateAppliesOverlayUntilExit`,
  - framework transition dispatch coverage added in `GestureBinding_HoverDispatchesPointerEnterAndPointerExitTransitions`.
- Added ink/ripple baseline:
  - introduced `RenderInkSplash` + `InkSplash` wrapper,
  - `MaterialButtonCore` now starts radial splash animation from pointer origin (`PointerDownEvent.LocalPosition`) and from center for keyboard activation,
  - regression coverage added in `TextButton_PointerDownStartsInkSplashRender`.
- Closed rounded-clip follow-up:
  - framework now includes rounded clip primitives (`ClipRRectLayer`, `ClipRRectOffsetLayer`, `RenderClipRRect`, `ClipRRect`, and `PaintingContext.PushClipRRect`),
  - `MaterialButtonCore` now wraps `InkSplash` with `ClipRRect` using button `BorderRadius` and disables internal rectangular splash clipping,
  - regression coverage added in `TextButton_UsesRoundedClipForInkSplash`, `PushClipRRect_CreatesClipRRectLayer_WithPictureChild`, and `ClipRRectWidget_CreatesRenderClipRRect_AndUpdatesBorderRadius`.
- Fixed pointer-click focus-overlay stickiness:
  - `MaterialButtonCore` now suppresses focus overlay after pointer activation, so background tint is released on `PointerUp`,
  - keyboard activation re-enables focus overlay behavior,
  - regression coverage added in `TextButton_PointerClick_DoesNotKeepFocusOverlayAfterPointerUp`.
- Improved ripple visibility on wide buttons:
  - splash alpha now remains stable through most of expansion and fades only in tail phase,
  - added layout regression `TextButton_TightWidth_ExpandsInkSplashToFullButtonBounds` to verify `RenderInkSplash` matches tight button width.
- Fixed resize-related ripple zone staleness:
  - `RenderClipRect`/`RenderClipRRect` now request composited-layer property refresh when implicit size-based clip bounds change in layout,
  - added regression `RenderClipRRect_UpdatesLayerClipRect_WhenSizeChanges` to verify clip bounds follow viewport resize.
- Added initial `ButtonStyle` infrastructure:
  - introduced `MaterialState`, `MaterialStateProperty<T>`, and `ButtonStyle` (`src/Flutter.Material/ButtonStyle.cs`),
  - `MaterialButtonCore` now resolves visual tokens from state-aware style properties (`foreground/background/overlay/splash/side/padding/shape/minimumSize`),
  - `TextButton`/`ElevatedButton`/`OutlinedButton` now accept `style` while keeping legacy constructor overrides compatible,
  - regression coverage added in `TextButton_ButtonStyleForegroundOverridesDefault`, `ElevatedButton_ButtonStyleMinimumSizeOverridesDefault`, and `OutlinedButton_ButtonStyleSideOverridesDefault`.
- Extended style ergonomics with `StyleFrom(...)` builders on all three button types:
  - added `TextButton.StyleFrom(...)`, `ElevatedButton.StyleFrom(...)`, and `OutlinedButton.StyleFrom(...)`,
  - added explicit style resolvers for overlay/splash colors,
  - regression coverage added in `TextButton_StyleFrom_AppliesForegroundAndTextStyle`, `ElevatedButton_StyleFrom_UsesDisabledColorOverrides`, and `TextButton_LegacyForeground_OverridesStyleFromForeground`.
- Closed overlay state-priority follow-up for button conflict cases:
  - default overlay resolver now matches Flutter priority order (`pressed > hovered > focused`),
  - regression coverage added in `TextButton_PressedOverlayTakesPriorityOverHoverOverlay` and `TextButton_HoverOverlayTakesPriorityOverFocusOverlay`.
- Continued `StyleFrom(...)` parity follow-up:
  - `foregroundColor` now derives default overlay/splash state colors when explicit `overlayColor`/`splashColor` are omitted,
  - explicit `overlayColor` now drives splash fallback when `splashColor` is omitted, including transparent highlight/splash suppression,
  - regression coverage added in `TextButton_StyleFrom_ForegroundColor_DerivesOverlayAndSplash` and `TextButton_StyleFrom_TransparentOverlay_DisablesVisualHighlights`.
- Added framework-level `State.StateWidget` protected accessor to support stateful widgets in external assemblies (`src/Flutter.Material`).
- Replaced sample shell `CounterTapButton` usage with Material buttons in both C# and Dart sample galleries (menu entries + back action), and switched Material-buttons-demo control-strip actions to `TextButton`.
- Added regression coverage for pressed-state visual transitions in `MaterialButtonsTests`.

## Test Plan

- Existing tests to run/update:
  - `dotnet test src/Flutter.Tests/Flutter.Tests.csproj -c Debug --filter "TextWidgetTests|MaterialScaffoldTests|MaterialButtonsTests"`
  - `dotnet build src/Sample/Flutter.Net/Flutter.Net.csproj -c Debug`
  - `dotnet build src/Sample/Flutter.Net.Desktop/Flutter.Net.Desktop.csproj -c Debug`
- New tests to add:
  - `src/Flutter.Tests/MaterialButtonsTests.cs`

## Sample Parity Plan

- [x] C# sample impact checked
- [x] Dart sample parity checked
- [x] `docs/ai/PARITY_MATRIX.md` updated (if needed)

## Docs and Tracking

- [x] `CHANGELOG.md` updated
- [x] `docs/FRAMEWORK_PLAN.md` status updated (if milestone/state changed)
- [x] `docs/ai/TEST_MATRIX.md` updated (if new coverage area was added)

## Done Criteria

- [x] Behavior implemented
- [x] Tests updated and passing
- [x] No invariant violations introduced
- [x] Parity constraints satisfied
