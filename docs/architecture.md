# PulseOps architecture

PulseOps starts deliberately small. The architecture grows only when a requirement gives us a reason to change it.

## M0: Foundation

```text
PulseOps.Web
     |
     | HTTP via Aspire service discovery
     v
PulseOps.Api
     |
     v
in-memory service registry
```

Aspire's AppHost starts both projects, wires the Web project to the API, exposes health information, and collects telemetry through Service Defaults.

## What is intentionally missing

M0 has no database, Redis, message broker, background worker, SignalR, or AI agent. Those will be introduced when the series reaches a problem that needs them.
