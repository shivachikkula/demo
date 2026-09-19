namespace DistrictPortal.Api.Services;

public interface IBlobStorageService
{
    /// <summary>Uploads <paramref name="content"/> to <paramref name="blobPath"/> and returns that same path.</summary>
    Task<string> UploadAsync(string blobPath, Stream content, string contentType, CancellationToken cancellationToken);

    Task<Stream> DownloadAsync(string blobPath, CancellationToken cancellationToken);
}
