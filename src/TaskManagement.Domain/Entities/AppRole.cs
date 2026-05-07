using Microsoft.AspNetCore.Identity;

namespace TaskManagement.Domain.Entities;

public class AppRole : IdentityRole<Guid>
{
    public string? Description { get; set; }

    public AppRole() { }

    public AppRole(string roleName) : base(roleName) { }
}
