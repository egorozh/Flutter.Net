# Porting Mode: Parity-First (Strict 1:1 Default)

Purpose: define a single mandatory workflow for Dart-to-C# control/widget ports.

## Source of Truth

- For framework controls/widgets that exist in Flutter, Dart implementation is the source of truth.
- Reference priority:
  1. Flutter framework Dart source (API/behavior/state/constraints defaults).
  2. `dart_sample` app structure and usage patterns.
  3. Existing C# implementation details (only when already parity-aligned).

## Default Porting Rule

- Default approach is structural `1:1` port, not behavioral approximation.
- Keep parity in:
  - public API shape and default values,
  - widget composition order,
  - state transitions and interaction states,
  - layout/constraints behavior,
  - paint/visual semantics.

## When Framework Primitives Are Missing

- Do not introduce control-local workaround logic if it changes structure/behavior from Flutter.
- First add or fix the missing framework primitive in `src/Flutter` / `src/Flutter.Material`.
- Then continue the control port with the same structure as Dart.

## Allowed Divergence

- Divergence is allowed only when platform/runtime constraints require it.
- Every divergence must be documented in the same iteration:
  - feature note (`docs/ai/*`),
  - `CHANGELOG.md` (short note),
  - inline code comment only when needed for future maintainers.
- Divergence note must include:
  - exact reason,
  - expected behavior delta,
  - follow-up condition for removing divergence.

## Required Validation for Ports

- Add/update focused tests in `src/Flutter.Tests` for:
  - default values,
  - interaction states,
  - known high-risk layout/constraints scenarios.
- Keep sample parity in the same iteration:
  - update both `src/Sample/Flutter.Net` and `dart_sample`,
  - update `docs/ai/PARITY_MATRIX.md` when route/page behavior changes.

## Definition of Done for Port Iterations

- Dart references are explicitly listed.
- C# structure follows Dart model without ad hoc substitutions.
- Divergences (if any) are documented with justification.
- Tests cover the parity-critical behavior introduced or changed.
