# PulseOps

[![CI](https://github.com/sahansera/PulseOps/actions/workflows/ci.yml/badge.svg)](https://github.com/sahansera/PulseOps/actions/workflows/ci.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

**An evolving .NET Aspire reliability platform for exploring distributed systems, observability, resilience, and agentic operations.**

PulseOps is built in public as the companion project for the [Building PulseOps](https://sahansera.dev/series/building-pulseops/) series.

The project starts deliberately small. New infrastructure gets added when a real requirement exposes a limitation in the current design, not because a getting-started guide had another box in its architecture diagram.

> **Project status:** early and evolving. Until `v1.0`, APIs, configuration and architecture may change between milestones. Published article tags remain immutable.

## What is PulseOps?

PulseOps is a small service reliability platform that will grow through a series of production-style problems:

- shared state across multiple application instances;
- durable incident and operational data;
- health, readiness and dependency behaviour;
- logs, traces and metrics;
- event-driven processing and resilience;
- agent-assisted incident investigation.

The goal is not to build every feature at once. Each milestone should make the next architectural decision explainable.

## Current architecture

M2 runs two API replicas against shared Redis and PostgreSQL resources:

```text
PulseOps.Web
     |
     | Aspire service discovery
     v
PulseOps.Api x 2
     |       \
     |        +--> Redis (shared short-lived service status)
     |
     +-----------> PostgreSQL (durable incidents and status history)

PulseOps.Migrations ---> PostgreSQL
```

Aspire provides local orchestration, health checks, service discovery, telemetry wiring
and the dashboard around those resources. It starts Redis and PostgreSQL, runs migrations
once, and supplies connection information to both API replicas; developers do not
configure localhost connection strings.

See [docs/architecture.md](docs/architecture.md) and the [roadmap](docs/roadmap.md) for where the system is heading.

## Requirements

- .NET 10 SDK
- Aspire CLI 13.4+
- Docker or another Aspire-compatible container runtime

Redis and PostgreSQL are part of the normal M2 runtime and are managed through the Aspire
AppHost. No separate database or cache setup is required.

## Run locally

From the repository root:

```bash
aspire run
```

Or run the AppHost directly:

```bash
dotnet run --project PulseOps.AppHost
```

The Aspire dashboard will expose the running resources and their endpoints.

## API

Service definitions remain process-local. Observed service status is shared through Redis
with a 30-second absolute expiration.

```http
GET  /services
GET  /services/{id}
POST /services
PUT  /services/{id}/status
```

Example:

```bash
curl -X POST http://localhost:<api-port>/services \
  -H 'Content-Type: application/json' \
  -d '{
    "id": "identity-api",
    "name": "Identity API",
    "url": "https://example.com/identity/health"
  }'
```

Write a short-lived status:

```bash
curl -X PUT http://localhost:<api-port>/services/payments-api/status \
  -H 'Content-Type: application/json' \
  -d '{ "status": "Healthy" }'
```

The write can be handled by one API replica and read from the other because both use the same Redis-backed `IDistributedCache`. A missing or expired status is returned as `Unknown`.

Incidents and their status history are durable PostgreSQL state:

```http
POST /incidents
GET  /incidents
GET  /incidents/{id}
PUT  /incidents/{id}/status
```

Create an incident:

```bash
curl -X POST http://localhost:<api-port>/incidents \
  -H 'Content-Type: application/json' \
  -d '{
    "serviceId": "payments-api",
    "summary": "Payments API unavailable"
  }'
```

## Tests

```bash
dotnet test
```

The integration tests start the Aspire AppHost and prove that two distinct API processes
share Redis status and PostgreSQL incidents. They also verify incident history and
durability across API process restarts.

## How the project is versioned

PulseOps separates the article narrative from the evolving project:

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

Article snapshots use milestone tags such as:

```text
pulseops-00-foundation
pulseops-01-redis
pulseops-02-postgres
pulseops-03-observability
```

A published tag is never moved or rewritten. `main` is always the latest development version.

See [docs/series.md](docs/series.md) for the article-to-snapshot mapping.

## Architecture decisions

Long-lived architecture decisions live under [`docs/adr/`](docs/adr/).

The blog explains the story and reasoning in depth. ADRs record the decision and its consequences for contributors working in the repository.

## Contributing

Contributions are welcome. Start with [CONTRIBUTING.md](CONTRIBUTING.md) and check the [roadmap](docs/roadmap.md) before proposing a large feature.

The project follows the series direction, so a technically impressive feature can still be too early. Kafka will survive another week.

## Security

Please report suspected vulnerabilities privately rather than opening a public issue. See [SECURITY.md](SECURITY.md).

## License

PulseOps is licensed under the [MIT License](LICENSE).
