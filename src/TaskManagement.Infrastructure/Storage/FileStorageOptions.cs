namespace TaskManagement.Infrastructure.Storage;

public sealed class FileStorageOptions
{
    public const string SectionName = "FileStorage";

    public string Root { get; set; } = "./uploads";
    public long MaxFileSizeBytes { get; set; } = 25 * 1024 * 1024; // 25 MB
}
