using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using TaskManagement.Application.Common.Exceptions;
using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Application.Common.Mappings;
using TaskManagement.Application.Features.Tasks.Commands.CreateTask;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Enums;
using TaskManagement.Infrastructure.Persistence;

namespace TaskManagement.Application.UnitTests.Features.Tasks;

public class CreateTaskCommandHandlerTests
{
    private readonly Guid _creatorId = Guid.NewGuid();

    [Fact]
    public async Task Handle_creates_a_task_for_the_current_user()
    {
        var (ctx, sut, _, _, _) = NewSut(authenticated: true);

        var command = new CreateTaskCommand(
            Title: "Write unit tests",
            Description: null,
            Status: TaskItemStatus.Todo,
            Priority: TaskPriority.High,
            DueDate: null,
            AssigneeId: null,
            ParentTaskId: null,
            LabelIds: null);

        var dto = await sut.Handle(command, CancellationToken.None);

        dto.Title.Should().Be("Write unit tests");
        dto.Priority.Should().Be(TaskPriority.High);
        dto.Status.Should().Be(TaskItemStatus.Todo);
        dto.CreatorId.Should().Be(_creatorId);

        ctx.Tasks.Should().ContainSingle(t => t.Title == "Write unit tests");
    }

    [Fact]
    public async Task Handle_throws_when_caller_is_anonymous()
    {
        var (_, sut, _, _, _) = NewSut(authenticated: false);

        var command = new CreateTaskCommand(
            Title: "anything",
            Description: null,
            Status: TaskItemStatus.Todo,
            Priority: TaskPriority.Low,
            DueDate: null,
            AssigneeId: null,
            ParentTaskId: null,
            LabelIds: null);

        await sut.Invoking(s => s.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<UnauthorizedException>();
    }

    [Fact]
    public async Task Handle_throws_when_assignee_does_not_exist()
    {
        var (_, sut, _, _, _) = NewSut(authenticated: true);

        var command = new CreateTaskCommand(
            Title: "with assignee",
            Description: null,
            Status: TaskItemStatus.Todo,
            Priority: TaskPriority.Low,
            DueDate: null,
            AssigneeId: Guid.NewGuid(),
            ParentTaskId: null,
            LabelIds: null);

        await sut.Invoking(s => s.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<BusinessRuleException>()
            .WithMessage("*Assignee does not exist*");
    }

    [Fact]
    public async Task Handle_starts_clock_when_status_is_InProgress()
    {
        var (ctx, sut, _, _, _) = NewSut(authenticated: true);

        var dto = await sut.Handle(new CreateTaskCommand(
            Title: "Already started",
            Description: null,
            Status: TaskItemStatus.InProgress,
            Priority: TaskPriority.Medium,
            DueDate: null,
            AssigneeId: null,
            ParentTaskId: null,
            LabelIds: null), CancellationToken.None);

        var saved = await ctx.Tasks.SingleAsync(t => t.Id == dto.Id);
        saved.StartedAt.Should().NotBeNull();
        saved.CompletedAt.Should().BeNull();
    }

    [Fact]
    public async Task Handle_notifies_assignee_when_assigned_to_someone_else()
    {
        var assigneeId = Guid.NewGuid();

        var (ctx, sut, notifications, _, _) = NewSut(authenticated: true);

        // seed assignee user so the existence guard passes
        ctx.Users.Add(new AppUser
        {
            Id = assigneeId,
            UserName = "assignee@x.test",
            Email = "assignee@x.test",
            FirstName = "A",
            LastName = "Ssignee",
            IsActive = true
        });
        await ctx.SaveChangesAsync();

        await sut.Handle(new CreateTaskCommand(
            Title: "for assignee",
            Description: null,
            Status: TaskItemStatus.Todo,
            Priority: TaskPriority.Medium,
            DueDate: null,
            AssigneeId: assigneeId,
            ParentTaskId: null,
            LabelIds: null), CancellationToken.None);

        notifications.Verify(n =>
            n.NotifyUserAsync(assigneeId, It.IsAny<Notification>(), It.IsAny<CancellationToken>()),
            Times.Once);

        ctx.Notifications.Should().ContainSingle(n => n.RecipientId == assigneeId && n.Type == NotificationType.TaskAssigned);
    }

    private (
        ApplicationDbContext db,
        CreateTaskCommandHandler handler,
        Mock<INotificationService> notifications,
        Mock<IActivityLogger> activity,
        Mock<ICurrentUserService> currentUser
    ) NewSut(bool authenticated)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"tasks-{Guid.NewGuid()}")
            .Options;
        var ctx = new ApplicationDbContext(options, Array.Empty<ISaveChangesInterceptor>());

        // Seed the creator user so post-save Includes hydrate the required Creator navigation.
        if (authenticated)
        {
            ctx.Users.Add(new AppUser
            {
                Id = _creatorId,
                UserName = "creator@x.test",
                Email = "creator@x.test",
                FirstName = "Test",
                LastName = "Creator",
                IsActive = true
            });
            ctx.SaveChanges();
        }

        var currentUser = new Mock<ICurrentUserService>();
        currentUser.SetupGet(c => c.UserId).Returns(authenticated ? _creatorId : (Guid?)null);
        currentUser.SetupGet(c => c.IsAuthenticated).Returns(authenticated);
        currentUser.SetupGet(c => c.Roles).Returns(Array.Empty<string>());
        currentUser.Setup(c => c.IsInRole(It.IsAny<string>())).Returns(false);

        var notifications = new Mock<INotificationService>();
        var activity = new Mock<IActivityLogger>();

        // Use the DI registration helper to avoid AutoMapper 14's constructor surface area.
        var services = new ServiceCollection();
        services.AddAutoMapper(cfg => cfg.AddProfile<MappingProfile>());
        var mapper = services.BuildServiceProvider().GetRequiredService<IMapper>();

        var handler = new CreateTaskCommandHandler(ctx, currentUser.Object, notifications.Object, activity.Object, mapper);
        return (ctx, handler, notifications, activity, currentUser);
    }
}
