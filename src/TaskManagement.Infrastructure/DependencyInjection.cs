using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Domain.Entities;
using TaskManagement.Infrastructure.Auditing;
using TaskManagement.Infrastructure.Authentication;
using TaskManagement.Infrastructure.Email;
using TaskManagement.Infrastructure.Identity;
using TaskManagement.Infrastructure.Persistence;
using TaskManagement.Infrastructure.Persistence.Interceptors;
using TaskManagement.Infrastructure.Persistence.Repositories;
using TaskManagement.Infrastructure.Persistence.Seed;
using TaskManagement.Infrastructure.Services;
using TaskManagement.Infrastructure.Storage;

namespace TaskManagement.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");

        services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();
        services.TryAddSingleton(TimeProvider.System);
        services.AddDataProtection();
        services.AddScoped<ISaveChangesInterceptor, AuditingSaveChangesInterceptor>();

        services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            options.UseSqlServer(connectionString, sql =>
            {
                sql.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
                sql.EnableRetryOnFailure(maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(10), errorNumbersToAdd: null);
            });
        });

        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());

        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddIdentityCore<AppUser>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequiredLength = 8;
                options.User.RequireUniqueEmail = true;
                options.SignIn.RequireConfirmedEmail = false;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                options.Lockout.MaxFailedAccessAttempts = 5;
            })
            .AddRoles<AppRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        // Options binding
        services.AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .Validate(o => !string.IsNullOrWhiteSpace(o.SigningKey) && o.SigningKey.Length >= 32,
                "Jwt:SigningKey must be configured and at least 32 characters long.")
            .Validate(o => !string.IsNullOrWhiteSpace(o.Issuer) && !string.IsNullOrWhiteSpace(o.Audience),
                "Jwt:Issuer and Jwt:Audience must be configured.");

        services.AddOptions<EmailOptions>()
            .Bind(configuration.GetSection(EmailOptions.SectionName));

        services.AddOptions<FileStorageOptions>()
            .Bind(configuration.GetSection(FileStorageOptions.SectionName));

        // Application abstractions ↦ Infrastructure implementations
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<IEmailSender, MailKitEmailSender>();
        services.AddScoped<IFileStorageService, LocalFileStorageService>();
        services.AddScoped<IActivityLogger, ActivityLogger>();

        services.AddScoped<DatabaseSeeder>();

        return services;
    }
}
