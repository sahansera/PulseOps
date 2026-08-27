# PulseOps architecture

PulseOps starts deliberately small. The architecture grows only when a requirement gives us a reason to change it.

## M1: Shared cache state

```text
PulseOps.Web
     |
     | HTTP via Aspire service discovery
     v
PulseOps.Api x 2
     |       \
     |        +--> process-local service definitions
     |
     | IDistributedCache
     v
Redis (shared service status, 30-second expiration)
```

Aspire's AppHost starts Redis, two API replicas, and the Web project. It supplies the Redis connection information to the API replicas, wires Web to the load-balanced API resource, exposes health information, and collects telemetry through Service Defaults.

## State ownership

PulseOps deliberately separates two kinds of state:

- Service definitions such as ID, name, and URL remain in each API process's local registry. Registering a definition through one replica does not make it available to the other replica yet.
- Observed service status is short-lived shared state. Both replicas read and write it through `IDistributedCache`, backed by the same Aspire-managed Redis resource.

Status entries use the namespaced key `service-status:{id}` and expire after 30 seconds. When no entry exists, the API returns `Unknown`.

Redis is not durable persistence in this architecture. Cached status can expire or disappear when Redis restarts, and that is acceptable for a recent observation that can be produced again. Incident history and other state that must survive restarts belong in a later durable-storage milestone.

## What is intentionally missing

M1 has no database, Redis persistence volume, message broker, background worker, SignalR, or AI agent. Those will be introduced only when the series reaches a problem that needs them.
