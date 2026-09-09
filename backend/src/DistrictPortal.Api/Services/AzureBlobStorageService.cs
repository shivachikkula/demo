using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Options;

namespace DistrictPortal.Api.Services;

public sealed class BlobStorageOptions
{
    public required string ConnectionString { get; set; }

    public string ContainerName { get; set; } = "submissions";
}

/// <summary>Stores uploaded submission files in an Azure Storage Account blob container.</summary>
public sealed class AzureBlobStorageService : IBlobStorageService
{
    private readonly BlobContainerClient _container;

    public AzureBlobStorageService(IOptions<BlobStorageOptions> options)
    {
        var config = options.Value;
        _container = new BlobContainerClient(config.ConnectionString, config.ContainerName);
    }

    public async Task<string> UploadAsync(string blobPath, Stream content, string contentType, CancellationToken cancellationToken)
    {
        await _container.CreateIfNotExistsAsync(PublicAccessType.None, cancellationToken: cancellationToken);

        var blob = _container.GetBlobClient(blobPath);
        await blob.UploadAsync(
            content,
            new BlobUploadOptions { HttpHeaders = new BlobHttpHeaders { ContentType = contentType } },
            cancellationToken);

        return blobPath;
    }

    public async Task<Stream> DownloadAsync(string blobPath, CancellationToken cancellationToken)
    {
        var blob = _container.GetBlobClient(blobPath);
        var download = await blob.DownloadStreamingAsync(cancellationToken: cancellationToken);
        return download.Value.Content;
    }
}
