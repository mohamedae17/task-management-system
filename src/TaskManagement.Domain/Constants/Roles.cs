namespace TaskManagement.Domain.Constants;

public static class Roles
{
    public const string Admin = nameof(Admin);
    public const string Manager = nameof(Manager);
    public const string Employee = nameof(Employee);

    public static readonly IReadOnlyList<string> All = new[] { Admin, Manager, Employee };
}
