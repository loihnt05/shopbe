using Microsoft.Extensions.Configuration;
using Shopbe.Application.Common.Interfaces;

namespace Shopbe.Infrastructure.Storage;

public sealed class LocalFileStorage(IConfiguration configuration) : IFileStorage
{
    public async Task<string> SaveAsync(Stream content, string fileName, string contentType, CancellationToken cancellationToken = default)
    {
        var uploadsSubFolder = configuration["Storage:UploadsFolder"];
        if (string.IsNullOrWhiteSpace(uploadsSubFolder))
            uploadsSubFolder = "uploads";
        var root = configuration["Storage:UploadsRoot"];
        if (string.IsNullOrWhiteSpace(root))
            root = Directory.GetCurrentDirectory();

        var uploadsRoot = Path.Combine(root, uploadsSubFolder);
        Directory.CreateDirectory(uploadsRoot);

        var safeExt = Path.GetExtension(fileName);
        if (string.IsNullOrWhiteSpace(safeExt)) safeExt = ".bin";

        var storedFileName = $"{Guid.NewGuid():N}{safeExt}";
        var storedPath = Path.Combine(uploadsRoot, storedFileName);

        await using (var fs = new FileStream(storedPath, FileMode.CreateNew, FileAccess.Write, FileShare.None, 81920, useAsync: true))
        {
            await content.CopyToAsync(fs, cancellationToken);
        }

        // Web project serves this folder via static files middleware.
        return $"/uploads/{storedFileName}";
    }
}


