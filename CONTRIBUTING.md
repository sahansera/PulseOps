# Contributing to PulseOps

Thanks for taking an interest in PulseOps.

PulseOps is built in public alongside the **Building PulseOps** article series. The project deliberately starts small and adds infrastructure only when a concrete requirement justifies it.

## Before opening a pull request

1. Check existing issues and the [roadmap](docs/roadmap.md).
2. For a significant architectural change, open an issue first so we can agree on the direction.
3. Keep changes focused. A small PR that solves one problem is easier to review and easier to explain.

The project follows the series direction. Large speculative additions may be technically interesting but still be out of scope for the current milestone.

## Local development

Requirements:

- .NET 10 SDK
- Aspire CLI 13.4+

Run the application from the repository root:

```bash
aspire run
```

Or run the AppHost directly:

```bash
dotnet run --project PulseOps.AppHost
```

Run the test suite with:

```bash
dotnet test
```

## Pull requests

A good pull request should:

- explain the problem it solves;
- keep unrelated refactoring out of the change;
- include tests when behaviour changes;
- update documentation when the public behaviour or architecture changes;
- call out any operational trade-offs introduced by the change.

Architecture decisions that change a system boundary or long-lived technical direction should include an ADR under `docs/adr/`.

## Article snapshots

`main` keeps evolving. Published articles point to immutable tags such as:

```text
pulseops-00-foundation
pulseops-01-redis
```

Do not move or rewrite an existing article tag. See [docs/series.md](docs/series.md) for the full policy.

## Reporting security issues

Please do not open a public issue for a suspected vulnerability. See [SECURITY.md](SECURITY.md).
