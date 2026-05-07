using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TaskManagement.Infrastructure.Persistence;

namespace TaskManagement.Api.IntegrationTests;

public sealed class TaskManagementWebApplicationFactory : WebApplicationFactory<Program>
{
    public string DatabaseName { get; } = $"itest-{Guid.NewGuid():N}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, cfg) =>
        {
            cfg.AddInMemoryCollection(new Dictionary<string, string?>
            {
                // Required by Infrastructure validators even though we swap out the provider below.
                ["ConnectionStrings:DefaultConnection"] = "Server=memory;",
                ["Jwt:Issuer"] = "TaskManagement.Tests",
                ["Jwt:Audience"] = "TaskManagement.Tests",
                ["Jwt:SigningKey"] = "test-signing-key-must-be-at-least-32-chars-long-for-validator",
                ["Jwt:AccessTokenMinutes"] = "30",
                ["Jwt:RefreshTokenDays"] = "14",
                ["Email:Mode"] = "PickupDirectory",
                ["Email:PickupPath"] = Path.Combine(Path.GetTempPath(), "tm-itest-mail"),
                ["FileStorage:Root"] = Path.Combine(Path.GetTempPath(), "tm-itest-uploads")
            });
        });

        builder.ConfigureServices(services =>
        {
            // Replace the SQL Server DbContext with an in-memory provider for tests.
            var dbContextDescriptors = services
                .Where(d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>))
                .ToList();
            foreach (var d in dbContextDescriptors) services.Remove(d);

            services.AddDbContext<ApplicationDbContext>((sp, options) =>
            {
                options.UseInMemoryDatabase(DatabaseName);
            });
        });
    }
}
