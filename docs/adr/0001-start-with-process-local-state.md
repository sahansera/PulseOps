# ADR 0001: Start with process-local state

**Status:** Accepted

## Context

The first PulseOps milestone needs a small service registry and enough application structure to exercise the Web, API and Aspire AppHost. There is no requirement yet for state to survive restarts or be shared across processes.

Adding a database or distributed cache at this stage would introduce infrastructure before the application has a requirement for it.

## Decision

PulseOps starts with process-local in-memory state.

The initial implementation should remain easy to run, easy to test and easy to replace when a later requirement changes the state ownership boundary.

## Consequences

- Local development has no database or cache dependency in M0.
- State is lost when the process restarts.
- State is not shared across multiple API instances.
- Scaling the API will deliberately expose the point where process-local state stops satisfying the system's requirements.

That limitation is expected, not a defect in the M0 architecture.
