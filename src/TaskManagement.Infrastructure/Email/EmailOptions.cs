namespace TaskManagement.Infrastructure.Email;

public sealed class EmailOptions
{
    public const string SectionName = "Email";

    /// "Smtp" sends through Host/Port; "PickupDirectory" writes .eml files
    /// to PickupPath (handy for local dev without an SMTP server).
    public string Mode { get; set; } = "PickupDirectory";

    public string FromAddress { get; set; } = "noreply@taskmgmt.local";
    public string FromName { get; set; } = "Task Management";

    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 25;
    public bool UseSsl { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }

    public string PickupPath { get; set; } = "./mail-pickup";
}
