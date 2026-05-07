namespace TaskManagement.Domain.Enums;

public enum ActivityAction
{
    Created = 1,
    Updated = 2,
    Deleted = 3,
    Restored = 4,
    Assigned = 5,
    Unassigned = 6,
    StatusChanged = 7,
    PriorityChanged = 8,
    DueDateChanged = 9,
    Commented = 10,
    AttachmentAdded = 11,
    AttachmentRemoved = 12,
    LabelAdded = 13,
    LabelRemoved = 14,
    Login = 15,
    Logout = 16,
    PasswordChanged = 17,
    RoleChanged = 18
}
