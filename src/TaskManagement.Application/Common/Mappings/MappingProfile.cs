using AutoMapper;
using TaskManagement.Application.Features.Comments.Dtos;
using TaskManagement.Application.Features.Labels.Dtos;
using TaskManagement.Application.Features.Notifications.Dtos;
using TaskManagement.Application.Features.Tasks.Dtos;
using TaskManagement.Application.Features.Users.Dtos;
using TaskManagement.Domain.Entities;

namespace TaskManagement.Application.Common.Mappings;

public sealed class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Users — roles are populated in handlers (UserManager call), so left unmapped here.
        CreateMap<AppUser, UserDto>()
            .ForMember(d => d.FullName, o => o.MapFrom(s => s.FullName))
            .ForMember(d => d.Roles, o => o.Ignore());

        CreateMap<AppUser, UserSummaryDto>()
            .ForMember(d => d.FullName, o => o.MapFrom(s => s.FullName));

        // Tasks
        CreateMap<TaskItem, TaskDto>()
            .ForMember(d => d.AssigneeName,
                o => o.MapFrom(s => s.Assignee != null ? s.Assignee.FullName : null))
            .ForMember(d => d.CreatorName,
                o => o.MapFrom(s => s.Creator.FullName))
            .ForMember(d => d.CommentCount,
                o => o.MapFrom(s => s.Comments.Count(c => !c.IsDeleted)))
            .ForMember(d => d.AttachmentCount,
                o => o.MapFrom(s => s.Attachments.Count(a => !a.IsDeleted)))
            .ForMember(d => d.Labels,
                o => o.MapFrom(s => s.TaskLabels.Select(tl => tl.Label)));

        CreateMap<TaskItem, TaskListItemDto>()
            .ForMember(d => d.AssigneeName,
                o => o.MapFrom(s => s.Assignee != null ? s.Assignee.FullName : null));

        // Comments
        CreateMap<Comment, CommentDto>()
            .ForMember(d => d.AuthorName, o => o.MapFrom(s => s.Author.FullName))
            .ForMember(d => d.AuthorAvatarUrl, o => o.MapFrom(s => s.Author.AvatarUrl));

        // Labels
        CreateMap<Label, LabelDto>();

        // Notifications
        CreateMap<Notification, NotificationDto>();
    }
}
