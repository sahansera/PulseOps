# PulseOps roadmap

PulseOps evolves through small, explainable milestones. The roadmap is directional rather than a promise of dates.

## Current direction

### M0 - Foundation

- .NET 10 solution
- Aspire AppHost and ServiceDefaults
- Web and API projects
- in-memory service registry
- integration tests

Snapshot: `pulseops-00-foundation`

### M1 - Shared cache state

- reproduce process-local cache divergence across API instances
- add Redis through the Aspire application model
- move service status caching to `IDistributedCache`
- verify separate API instances observe the same cached state

Snapshot target: `pulseops-01-redis`

### M2 - Durable incident state

- add PostgreSQL through Aspire
- persist incidents and status history
- define migration ownership
- model startup, readiness and database failure behaviour

Snapshot target: `pulseops-02-postgres`

### M3 - Observability

- structured logs
- distributed traces
- metrics
- controlled failure scenarios that can be followed through the Aspire dashboard

Snapshot target: `pulseops-03-observability`

### M4 - Agent-assisted operations

- integrate the GitHub Copilot SDK as an agent runtime
- investigate incidents using controlled tools and application context
- connect agent reasoning to useful operational telemetry

Snapshot target: `pulseops-04-copilot-agent`

## Later ideas

These are intentionally not scheduled yet:

- event-driven alert ingestion
- background workers and messaging
- SignalR updates
- Microsoft Agent Framework
- Aspire MCP integration
- agent tracing and evaluation
- deployment and production topology
- reusable PulseOps packages where a real consumer boundary emerges

## How work enters the roadmap

A capability should solve a visible requirement or failure mode before it becomes infrastructure in the project.

For substantial proposals, open an issue describing:

1. the problem;
2. the behaviour we need;
3. the proposed mechanism;
4. alternatives considered;
5. the operational cost introduced by the change.

That keeps PulseOps useful as an engineering project rather than a collection of technologies looking for somewhere to live.
