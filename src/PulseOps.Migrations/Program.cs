using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PulseOps.Data;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();
builder.AddNpgsqlDbContext<PulseOpsDbContext>("pulseops");

var host = builder.Build();

await using var scope = host.Services.CreateAsyncScope();
var db = scope.ServiceProvider.GetRequiredService<PulseOpsDbContext>();

await db.Database.MigrateAsync();

return 0;
