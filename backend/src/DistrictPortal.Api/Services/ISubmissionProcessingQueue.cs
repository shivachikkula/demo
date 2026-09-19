namespace DistrictPortal.Api.Services;

public interface ISubmissionProcessingQueue
{
    void Enqueue(Guid submissionId);

    IAsyncEnumerable<Guid> ReadAllAsync(CancellationToken cancellationToken);
}
