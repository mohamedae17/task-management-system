using Microsoft.Extensions.Options;
using TaskManagement.Application.Common.Exceptions;
using TaskManagement.Application.Common.Interfaces;

namespace TaskManagement.Infrastructure.Storage;

public sealed class LocalFileStorageService : IFileStorageService
{
    private readonly FileStorageOptions _options;

    public LocalFileStorageService(IOptions<FileStorageOptions> options)
    {
        _options = options.Value;
    }

    public async Task<StoredFileInfo> SaveAsync(
        Stream content, string fileName, string contentType, string subfolder,
        CancellationToken cancellationToken = default)
    {
        if (!content.CanRead)
            throw new BusinessRuleException("Source stream cannot be read.");

        var safeName = SanitizeFileName(fileName);
        var ext = Path.GetExtension(safeName);
        var stored = $"{Guid.NewGuid():N}{ext}";

        var folder = Path.GetFullPath(Path.Combine(_options.Root, SanitizeSubfolder(subfolder)));
        Directory.CreateDirectory(folder);

        var path = Path.Combine(folder, stored);

        await using var fs = File.Create(path);
        await content.CopyToAsync(fs, cancellationToken);
        await fs.FlushAsync(cancellationToken);

        var size = new FileInfo(path).Length;
        if (size > _options.MaxFileSizeBytes)
        {
            File.Delete(path);
            throw new BusinessRuleException($"File exceeds the maximum size of {_options.MaxFileSizeBytes} bytes.");
        }

        // Persist a relative path so the storage root can be re-rooted later.
        var relative = Path.GetRelativePath(_options.Root, path).Replace('\\', '/');
        return new StoredFileInfo(relative, size, contentType);
    }

    public Task<Stream> OpenReadAsync(string storagePath, CancellationToken cancellationToken = default)
    {
        var fullPath = ResolveAndGuardPath(storagePath);
        if (!File.Exists(fullPath))
            throw new NotFoundException($"File '{storagePath}' was not found.");

        Stream s = File.OpenRead(fullPath);
        return Task.FromResult(s);
    }

    public Task DeleteAsync(string storagePath, CancellationToken cancellationToken = default)
    {
        var fullPath = ResolveAndGuardPath(storagePath);
        if (File.Exists(fullPath)) File.Delete(fullPath);
        return Task.CompletedTask;
    }

    private string ResolveAndGuardPath(string storagePath)
    {
        var rootFull = Path.GetFullPath(_options.Root);
        var combined = Path.GetFullPath(Path.Combine(rootFull, storagePath));
        // Reject path traversal — combined must stay rooted under the storage root.
        if (!combined.StartsWith(rootFull, StringComparison.OrdinalIgnoreCase))
            throw new BusinessRuleException("Invalid storage path.");
        return combined;
    }

    private static string SanitizeFileName(string fileName)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var cleaned = new string(fileName.Where(c => !invalid.Contains(c)).ToArray());
        return string.IsNullOrWhiteSpace(cleaned) ? "file" : cleaned;
    }

    private static string SanitizeSubfolder(string subfolder)
    {
        var invalid = Path.GetInvalidFileNameChars().Concat(new[] { '/', '\\' }).ToArray();
        var cleaned = new string(subfolder.Where(c => !invalid.Contains(c)).ToArray());
        return string.IsNullOrWhiteSpace(cleaned) ? "misc" : cleaned;
    }
}
