namespace TaskManagement.Application.Common.Interfaces;

public sealed record StoredFileInfo(string StoragePath, long SizeBytes, string ContentType);

public interface IFileStorageService
{
    Task<StoredFileInfo> SaveAsync(
        Stream content,
        string fileName,
        string contentType,
        string subfolder,
        CancellationToken cancellationToken = default);

    Task<Stream> OpenReadAsync(string storagePath, CancellationToken cancellationToken = default);

    Task DeleteAsync(string storagePath, CancellationToken cancellationToken = default);
}
