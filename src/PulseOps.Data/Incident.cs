namespace PulseOps.Data;

public enum IncidentStatus
{
    Open,
    Resolved
}

public sealed class Incident
{
    public Guid Id { get; set; }

    public required string ServiceId { get; set; }

    public required string Summary { get; set; }

    public IncidentStatus Status { get; set; }

    public DateTimeOffset CreatedAtUtc { get; set; }

    public DateTimeOffset UpdatedAtUtc { get; set; }

    public List<IncidentStatusHistory> History { get; set; } = [];
}

public sealed class IncidentStatusHistory
{
    public Guid Id { get; set; }

    public Guid IncidentId { get; set; }

    public IncidentStatus Status { get; set; }

    public DateTimeOffset ChangedAtUtc { get; set; }

    public Incident Incident { get; set; } = null!;
}
