using Microsoft.EntityFrameworkCore;
using PulseOps.Data;

namespace PulseOps.Api;

public sealed record CreateIncidentRequest(string ServiceId, string Summary);

public sealed record UpdateIncidentStatusRequest(string Status);

public sealed record IncidentStatusHistoryResponse(
    Guid Id,
    IncidentStatus Status,
    DateTimeOffset ChangedAtUtc);

public sealed record IncidentResponse(
    Guid Id,
    string ServiceId,
    string Summary,
    IncidentStatus Status,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc,
    IReadOnlyList<IncidentStatusHistoryResponse> History);

public static class IncidentEndpoints
{
    public static RouteGroupBuilder MapIncidentEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var incidents = endpoints.MapGroup("/incidents");

        incidents.MapGet("", GetIncidentsAsync);
        incidents.MapGet("/{id:guid}", GetIncidentAsync);
        incidents.MapPost("", CreateIncidentAsync);
        incidents.MapPut("/{id:guid}/status", UpdateIncidentStatusAsync);

        return incidents;
    }

    private static async Task<IResult> GetIncidentsAsync(
        PulseOpsDbContext db,
        CancellationToken cancellationToken)
    {
        var incidents = await db.Incidents
            .AsNoTracking()
            .Include(incident => incident.History)
            .OrderByDescending(incident => incident.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        return Results.Ok(incidents.Select(ToResponse));
    }

    private static async Task<IResult> GetIncidentAsync(
        Guid id,
        PulseOpsDbContext db,
        CancellationToken cancellationToken)
    {
        var incident = await db.Incidents
            .AsNoTracking()
            .Include(item => item.History)
            .SingleOrDefaultAsync(item => item.Id == id, cancellationToken);

        return incident is null
            ? Results.NotFound()
            : Results.Ok(ToResponse(incident));
    }

    private static async Task<IResult> CreateIncidentAsync(
        CreateIncidentRequest request,
        PulseOpsDbContext db,
        TimeProvider timeProvider,
        CancellationToken cancellationToken)
    {
        var validationErrors = Validate(request);
        if (validationErrors.Count > 0)
        {
            return Results.ValidationProblem(validationErrors);
        }

        var now = timeProvider.GetUtcNow();
        var incident = new Incident
        {
            Id = Guid.NewGuid(),
            ServiceId = request.ServiceId.Trim(),
            Summary = request.Summary.Trim(),
            Status = IncidentStatus.Open,
            CreatedAtUtc = now,
            UpdatedAtUtc = now,
            History =
            [
                new IncidentStatusHistory
                {
                    Id = Guid.NewGuid(),
                    Status = IncidentStatus.Open,
                    ChangedAtUtc = now
                }
            ]
        };

        db.Incidents.Add(incident);
        await db.SaveChangesAsync(cancellationToken);

        return Results.Created($"/incidents/{incident.Id}", ToResponse(incident));
    }

    private static async Task<IResult> UpdateIncidentStatusAsync(
        Guid id,
        UpdateIncidentStatusRequest request,
        PulseOpsDbContext db,
        TimeProvider timeProvider,
        CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<IncidentStatus>(request.Status, true, out var status))
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                ["status"] = ["Status must be Open or Resolved."]
            });
        }

        var incident = await db.Incidents
            .Include(item => item.History)
            .SingleOrDefaultAsync(item => item.Id == id, cancellationToken);

        if (incident is null)
        {
            return Results.NotFound();
        }

        if (incident.Status != status)
        {
            var now = timeProvider.GetUtcNow();
            incident.Status = status;
            incident.UpdatedAtUtc = now;
            incident.History.Add(new IncidentStatusHistory
            {
                Id = Guid.NewGuid(),
                Status = status,
                ChangedAtUtc = now
            });

            await db.SaveChangesAsync(cancellationToken);
        }

        return Results.Ok(ToResponse(incident));
    }

    private static Dictionary<string, string[]> Validate(CreateIncidentRequest request)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(request.ServiceId) || request.ServiceId.Trim().Length > 100)
        {
            errors["serviceId"] = ["ServiceId is required and must be at most 100 characters."];
        }

        if (string.IsNullOrWhiteSpace(request.Summary) || request.Summary.Trim().Length > 500)
        {
            errors["summary"] = ["Summary is required and must be at most 500 characters."];
        }

        return errors;
    }

    private static IncidentResponse ToResponse(Incident incident) =>
        new(
            incident.Id,
            incident.ServiceId,
            incident.Summary,
            incident.Status,
            incident.CreatedAtUtc,
            incident.UpdatedAtUtc,
            incident.History
                .OrderBy(item => item.ChangedAtUtc)
                .Select(item => new IncidentStatusHistoryResponse(
                    item.Id,
                    item.Status,
                    item.ChangedAtUtc))
                .ToArray());
}
