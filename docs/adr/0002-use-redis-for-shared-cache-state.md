# ADR 0002: Use Redis for shared cache state

**Status:** Accepted

## Context

M0 keeps all state inside one API process. Once PulseOps runs multiple API replicas, each process can hold a different cached status for the same service. Both caches may be locally correct while the system returns different answers depending on which replica handles a request.

The service registry and observed service status have different ownership and lifetime requirements. Service definitions can remain process-local for now, but short-lived status must be visible to every API replica.

## Decision

PulseOps will use Redis through the Aspire application model for shared cache state. The AppHost owns the Redis resource named `cache`, and both API replicas reference it without manually configured localhost connection strings.

Application code will access Redis through `IDistributedCache`. A focused `ServiceStatusCache` will store only observed service status under namespaced keys using explicit JSON serialization and a 30-second absolute expiration.

The process-local service registry will continue to own service IDs, names, and URLs until a durable-storage requirement changes that boundary.

## Consequences

- API replicas observe the same cached service status.
- Redis becomes a runtime dependency and requires a container runtime for local development.
- Cached status remains ephemeral and may expire or disappear when Redis restarts.
- Redis caching does not replace durable storage for incidents, status history, or service definitions.
- Key naming, serialization compatibility, and expiration policy are now part of the shared-state contract.
