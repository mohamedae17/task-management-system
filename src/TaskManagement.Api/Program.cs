using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using TaskManagement.Api.Hubs;
using TaskManagement.Api.Middleware;
using TaskManagement.Api.Services;
using TaskManagement.Application;
using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Infrastructure;
using TaskManagement.Infrastructure.Authentication;
using TaskManagement.Infrastructure.Persistence.Seed;

const string CorsPolicy = "TaskManagementCors";

var builder = WebApplication.CreateBuilder(args);

// Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithEnvironmentName()
    .WriteTo.Console()
    .WriteTo.File("logs/api-.log", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 14)
    .CreateLogger();
builder.Host.UseSerilog();

// CORS
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? new[] { "http://localhost:4200" };
builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicy, p => p
        .WithOrigins(allowedOrigins)
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials());
});

// MVC + behaviors
builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, HttpCurrentUserService>();

// Application + Infrastructure
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// SignalR + the API-side INotificationService implementation
builder.Services.AddSignalR();
builder.Services.AddScoped<INotificationService, SignalRNotificationService>();

// JWT auth
var jwt = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
    ?? throw new InvalidOperationException("Jwt configuration section is missing.");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwt.Issuer,
            ValidAudience = jwt.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey)),
            ClockSkew = TimeSpan.FromSeconds(30),
            NameClaimType = System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.UniqueName,
            RoleClaimType = System.Security.Claims.ClaimTypes.Role
        };

        // Allow SignalR clients to pass the access token via the query string on the hub URL.
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = ctx =>
            {
                var accessToken = ctx.Request.Query["access_token"];
                var path = ctx.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments(NotificationsHub.Path))
                    ctx.Token = accessToken;
                return Task.CompletedTask;
            }
        };
    });
builder.Services.AddAuthorization();

// Swagger with JWT bearer support
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Task Management API",
        Version = "v1",
        Description = "Production-ready task management backend (Clean Architecture + CQRS)."
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Paste the access token returned by /api/auth/login."
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });

    c.EnableAnnotations();
});

var app = builder.Build();

// One-shot CLI: `dotnet run --project src/TaskManagement.Api -- --seed`
if (args.Contains("--seed"))
{
    using var scope = app.Services.CreateScope();
    var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
    await seeder.SeedAsync();
    Log.Information("Seeding completed.");
    return;
}

app.UseSerilogRequestLogging();
app.UseMiddleware<GlobalExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Task Management API v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();
app.UseCors(CorsPolicy);
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => Results.Ok(new
{
    name = "TaskManagement.Api",
    version = "1.0.0",
    docs = "/swagger",
    hubs = new[] { NotificationsHub.Path }
})).AllowAnonymous();

app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTimeOffset.UtcNow }))
   .AllowAnonymous();

app.MapControllers();
app.MapHub<NotificationsHub>(NotificationsHub.Path);

try
{
    Log.Information("Starting Task Management API");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

public partial class Program { }
