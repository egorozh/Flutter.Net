# Feature Template

Use this template for every non-trivial feature iteration.

```md
# Feature: <short-name>

## Goal

- What user-visible or framework-level outcome should exist after this change?

## Non-Goals

- What is explicitly out of scope for this iteration?

## Context Budget Plan

- Budget: max <N> files in initial read (recommended: 6-8).
- Entry files:
  - <file 1>
  - <file 2>
  - <file 3>
- Expansion trigger:
  - Which concrete blocker allows opening more files?

## Invariants Impacted

- [ ] `docs/ai/INVARIANTS.md` reviewed
- List invariants that this feature touches:
  - <invariant A>
  - <invariant B>

## Planned Changes

- Files to edit:
  - <file path>
  - <file path>
- Brief intent per file:
  - <path>: <intent>

## Test Plan

- Existing tests to run/update:
  - <test file path>
- New tests to add:
  - <test file path>

## Sample Parity Plan

- [ ] C# sample impact checked
- [ ] Dart sample parity checked
- [ ] `docs/ai/PARITY_MATRIX.md` updated (if needed)

## Docs and Tracking

- [ ] `CHANGELOG.md` updated
- [ ] `docs/FRAMEWORK_PLAN.md` status updated (if milestone/state changed)
- [ ] `docs/ai/TEST_MATRIX.md` updated (if new coverage area was added)

## Done Criteria

- [ ] Behavior implemented
- [ ] Tests updated and passing
- [ ] No invariant violations introduced
- [ ] Parity constraints satisfied
```

## Optional Naming Convention

- Suggested branch prefix: `codex/<area>-<feature>`
- Suggested feature id format: `<area>-<yyyy-mm-dd>-<slug>`
