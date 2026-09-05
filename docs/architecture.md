# PulseOps architecture

PulseOps starts deliberately small. The architecture grows only when a requirement gives us a reason to change it.

## M2: Durable incident state

```text
PulseOps.Web
     |
     | HTTP via Aspire service discovery
     v
PulseOps.Api x 2
     |       \
     |        +--> Redis
     |             shared short-lived service status
     |
     +-----------> PostgreSQL
                   durable incidents + status history

PulseOps.Migrations
     |
     +-----------> PostgreSQL
```

Aspire's AppHost starts Redis, PostgreSQL, a one-shot migration process, two API replicas,
and the Web project. It supplies dependency connection information, waits for migrations
to complete before starting the API replicas, wires Web to the load-balanced API resource,
exposes health information, and collects telemetry through Service Defaults.

## State ownership

PulseOps deliberately separates two kinds of state:

- Service definitions such as ID, name, and URL remain in each API process's local registry. Registering a definition through one replica does not make it available to the other replica yet.
- Observed service status is short-lived shared state. Both replicas read and write it through `IDistributedCache`, backed by the same Aspire-managed Redis resource.
- Incidents and incident status history are durable state. Both replicas read and write
  them through EF Core, backed by the same Aspire-managed PostgreSQL database.

Status entries use the namespaced key `service-status:{id}` and expire after 30 seconds. When no entry exists, the API returns `Unknown`.

Redis is not durable persistence in this architecture. Cached status can expire or
disappear when Redis restarts, and that is acceptable for a recent observation that can
be produced again. Incident history must survive API and cache restarts, so PostgreSQL
owns it.

## Migration ownership

`PulseOps.Migrations` is the only process that applies EF Core migrations. Both API
replicas wait for it to exit successfully and never call `Database.Migrate()` themselves.
This keeps one-shot schema mutation out of replicated request-serving startup.

## What is intentionally missing

M2 has no message broker, background worker, SignalR, event sourcing, production
high-availability database topology, or AI agent. Those will be introduced only when the
series reaches a problem that needs them.
