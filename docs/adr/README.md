# Architecture decision records

PulseOps records architectural decisions that establish a long-lived system boundary, dependency or operating model.

ADRs are intentionally short. The blog series may tell the full story; the ADR records the decision for contributors working in the repository.

## Format

Use the next number in sequence:

```text
0001-start-with-process-local-state.md
0002-use-redis-for-shared-cache-state.md
```

Each ADR should contain:

- **Status** - proposed, accepted, superseded or rejected
- **Context** - the requirement or constraint
- **Decision** - what the project will do
- **Consequences** - useful trade-offs and costs

Accepted ADRs are not rewritten to make history look cleaner. If the project changes direction, add a new ADR and mark the old one as superseded.
