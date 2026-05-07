using Microsoft.Extensions.DependencyInjection;
using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Infrastructure;
using TaskManagement.Infrastructure.Persistence.Seed;
using TaskManagement.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration);

// Until the API layer wires up an HttpContext-backed implementation in Turn 3,
// the anonymous fallback keeps the auditing interceptor happy.
builder.Services.AddScoped<ICurrentUserService, AnonymousCurrentUserService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/", () => Results.Ok(new
{
    name = "TaskManagement.Api",
    status = "scaffold",
    note = "Auth, controllers, SignalR, and CQRS handlers land in Turn 3."
}));

app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTimeOffset.UtcNow }));

if (args.Contains("--seed"))
{
    using var scope = app.Services.CreateScope();
    var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
    await seeder.SeedAsync();
    return;
}

app.Run();

public partial class Program { }
