using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TaskManagement.Domain.Constants;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Enums;

namespace TaskManagement.Infrastructure.Persistence.Seed;

public sealed class DatabaseSeeder
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<AppUser> _userManager;
    private readonly RoleManager<AppRole> _roleManager;
    private readonly ILogger<DatabaseSeeder> _logger;

    public const string DefaultAdminEmail = "admin@taskmgmt.local";
    public const string DefaultAdminPassword = "Admin@123!";

    public const string DefaultManagerEmail = "manager@taskmgmt.local";
    public const string DefaultManagerPassword = "Manager@123!";

    public const string DefaultEmployeeEmail = "employee@taskmgmt.local";
    public const string DefaultEmployeePassword = "Employee@123!";

    public DatabaseSeeder(
        ApplicationDbContext db,
        UserManager<AppUser> userManager,
        RoleManager<AppRole> roleManager,
        ILogger<DatabaseSeeder> logger)
    {
        _db = db;
        _userManager = userManager;
        _roleManager = roleManager;
        _logger = logger;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await _db.Database.MigrateAsync(cancellationToken);

        await SeedRolesAsync();
        var admin = await SeedUserAsync(
            DefaultAdminEmail, DefaultAdminPassword, "System", "Admin", Roles.Admin);
        var manager = await SeedUserAsync(
            DefaultManagerEmail, DefaultManagerPassword, "Demo", "Manager", Roles.Manager);
        var employee = await SeedUserAsync(
            DefaultEmployeeEmail, DefaultEmployeePassword, "Demo", "Employee", Roles.Employee);

        await SeedLabelsAsync(cancellationToken);
        await SeedDemoTasksAsync(admin.Id, manager.Id, employee.Id, cancellationToken);
    }

    private async Task SeedRolesAsync()
    {
        foreach (var role in Roles.All)
        {
            if (!await _roleManager.RoleExistsAsync(role))
            {
                var result = await _roleManager.CreateAsync(new AppRole(role)
                {
                    Description = $"{role} role"
                });
                if (!result.Succeeded)
                {
                    _logger.LogError("Failed to seed role {Role}: {Errors}",
                        role, string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }
        }
    }

    private async Task<AppUser> SeedUserAsync(
        string email, string password, string firstName, string lastName, string role)
    {
        var existing = await _userManager.FindByEmailAsync(email);
        if (existing is not null)
        {
            if (!await _userManager.IsInRoleAsync(existing, role))
                await _userManager.AddToRoleAsync(existing, role);
            return existing;
        }

        var user = new AppUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            FirstName = firstName,
            LastName = lastName,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var create = await _userManager.CreateAsync(user, password);
        if (!create.Succeeded)
        {
            var errors = string.Join(", ", create.Errors.Select(e => e.Description));
            _logger.LogError("Failed to seed user {Email}: {Errors}", email, errors);
            throw new InvalidOperationException($"Could not seed user {email}: {errors}");
        }

        await _userManager.AddToRoleAsync(user, role);
        _logger.LogInformation("Seeded user {Email} with role {Role}", email, role);
        return user;
    }

    private async Task SeedLabelsAsync(CancellationToken cancellationToken)
    {
        if (await _db.Labels.AnyAsync(cancellationToken)) return;

        var labels = new[]
        {
            new Label { Name = "Bug",         ColorHex = "#E53935", Description = "Something isn't working" },
            new Label { Name = "Feature",     ColorHex = "#1E88E5", Description = "New capability" },
            new Label { Name = "Improvement", ColorHex = "#43A047", Description = "Enhancement to existing functionality" },
            new Label { Name = "Documentation", ColorHex = "#8E24AA", Description = "Docs and writing" },
            new Label { Name = "Urgent",      ColorHex = "#FB8C00", Description = "Needs immediate attention" }
        };

        await _db.Labels.AddRangeAsync(labels, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
    }

    private async Task SeedDemoTasksAsync(
        Guid adminId, Guid managerId, Guid employeeId, CancellationToken cancellationToken)
    {
        if (await _db.Tasks.AnyAsync(cancellationToken)) return;

        var now = DateTime.UtcNow;
        var tasks = new[]
        {
            new TaskItem
            {
                Title = "Set up project repository",
                Description = "Initial scaffolding for the task management system.",
                Status = TaskItemStatus.Completed,
                Priority = TaskPriority.High,
                CreatorId = adminId,
                AssigneeId = adminId,
                StartedAt = now.AddDays(-3),
                CompletedAt = now.AddDays(-1)
            },
            new TaskItem
            {
                Title = "Design database schema",
                Description = "Tables, indexes, relationships.",
                Status = TaskItemStatus.Completed,
                Priority = TaskPriority.High,
                CreatorId = adminId,
                AssigneeId = managerId,
                StartedAt = now.AddDays(-2),
                CompletedAt = now.AddHours(-12)
            },
            new TaskItem
            {
                Title = "Implement authentication endpoints",
                Description = "JWT + refresh token flow.",
                Status = TaskItemStatus.InProgress,
                Priority = TaskPriority.Critical,
                CreatorId = managerId,
                AssigneeId = employeeId,
                DueDate = now.AddDays(3),
                StartedAt = now.AddHours(-6)
            },
            new TaskItem
            {
                Title = "Build dashboard charts",
                Description = "Tasks-by-status and tasks-by-priority charts on the home page.",
                Status = TaskItemStatus.Todo,
                Priority = TaskPriority.Medium,
                CreatorId = managerId,
                AssigneeId = employeeId,
                DueDate = now.AddDays(7)
            },
            new TaskItem
            {
                Title = "Write README and run instructions",
                Description = "Document setup, seeded accounts, docker-compose flow.",
                Status = TaskItemStatus.Todo,
                Priority = TaskPriority.Low,
                CreatorId = adminId,
                AssigneeId = managerId,
                DueDate = now.AddDays(14)
            }
        };

        await _db.Tasks.AddRangeAsync(tasks, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
    }
}
