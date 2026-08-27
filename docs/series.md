# Building PulseOps

PulseOps is the open-source companion project for the **Building PulseOps** series on [sahansera.dev](https://sahansera.dev/series/building-pulseops/).

The blog and repository have different jobs:

```text
Blog article
    | explains why
    v
GitHub Release
    | explains what changed
    v
Git tag
    | preserves exact code
    v
main
    keeps evolving
```

## Milestones

| Milestone | Topic | Start tag | End tag | Status |
|---|---|---|---|---|
| M0 | Foundation | - | `pulseops-00-foundation` | Previous |
| M1 | From in-memory caching to Redis | `pulseops-00-foundation` | `pulseops-01-redis` (after merge) | Current implementation; tag pending |
| M2 | Durable incident state with PostgreSQL | `pulseops-01-redis` | `pulseops-02-postgres` | Planned |
| M3 | Logs, traces and metrics | `pulseops-02-postgres` | `pulseops-03-observability` | Planned |
| M4 | GitHub Copilot SDK incident agent | `pulseops-03-observability` | `pulseops-04-copilot-agent` | Planned |

Later milestones may add event-driven alert processing, background workers, messaging, SignalR, Microsoft Agent Framework, MCP, agent tracing, evaluation and deployment.

M1 is implemented in the current codebase, but `pulseops-01-redis` must not be created until the M1 pull request is reviewed and merged. Until then, `pulseops-00-foundation` remains the latest published immutable snapshot.

## Snapshot policy

Every substantial article has an exact before and after snapshot.

For example:

```text
Starting point: pulseops-00-foundation
Finished version: pulseops-01-redis
```

Rules:

1. `main` is allowed to keep changing.
2. Published `pulseops-*` tags are immutable.
3. Articles link to exact tags rather than only to `main`.
4. Each article tag gets a GitHub Release.
5. The release describes what changed and links back to the article.
6. If an old snapshot contains a later-discovered problem, document it rather than rewriting history.

## Releases

Pushing a `pulseops-*` tag triggers the release workflow and creates a GitHub Release with generated notes.

Before publishing the corresponding article, edit the release notes so they include:

- the article title and link;
- the problem the milestone solves;
- the key architectural change;
- how to run or verify the snapshot;
- the previous snapshot link.

Release titles can stay aligned with their immutable tags. Once PulseOps becomes something users deploy or consume as a versioned product, semantic versions such as `v0.1.0` can be introduced separately without replacing the article snapshots.
