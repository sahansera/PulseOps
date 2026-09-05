# ADR 0003: Use PostgreSQL for durable incident state

- Status: Accepted
- Date: 2026-09-05

## Context

Redis gives both API replicas one shared view of short-lived service status. That state is
replaceable: PulseOps can probe a service again after an entry expires or Redis restarts.

Incidents and their status history are different. They must survive API restarts,
deployments, cache expiration, and Redis loss. Storing them in process memory or the
existing cache would make operational history disposable.

Running EF Core migrations from API startup would also give both replicas permission to
change the schema concurrently.

## Decision

PulseOps will use an Aspire-managed PostgreSQL database for incidents and incident status
history. Redis will continue to own short-lived service status.

The API and a dedicated migration process share the EF Core model from `PulseOps.Data`.
Only `PulseOps.Migrations` calls `Database.MigrateAsync()`. The AppHost waits for that
one-shot process to finish successfully before starting the two API replicas.

PostgreSQL uses an Aspire data volume for local development. Aspire supplies connection
information to the API and migration projects, so the normal workflow does not require a
hand-managed localhost connection string.

## Consequences

- Incident state remains available independently of the API process that wrote it.
- Both API replicas observe the same incidents and status history.
- Schema mutation has one explicit owner.
- Database availability is now part of API readiness and incident endpoint availability.
- PulseOps gains PostgreSQL operations, backup, migration, and capacity concerns.
- Service definitions remain process-local, and service status remains replaceable Redis
  state.
