namespace Shopbe.Application.Common.Interfaces;

public interface IFileStorage
{
    /// <summary>
    /// Stores a file and returns a public URL that can be used by clients.
    /// </summary>
    Task<string> SaveAsync(
        Stream content,
        string fileName,
        string contentType,
        CancellationToken cancellationToken = default);
}

