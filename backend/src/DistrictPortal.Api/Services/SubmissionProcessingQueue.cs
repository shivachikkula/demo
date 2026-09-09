using System.Threading.Channels;

namespace DistrictPortal.Api.Services;

/// <summary>In-process work queue handing newly uploaded submissions off to the background worker.</summary>
public sealed class SubmissionProcessingQueue : ISubmissionProcessingQueue
{
    private readonly Channel<Guid> _channel = Channel.CreateUnbounded<Guid>();

    public void Enqueue(Guid submissionId) => _channel.Writer.TryWrite(submissionId);

    public IAsyncEnumerable<Guid> ReadAllAsync(CancellationToken cancellationToken) =>
        _channel.Reader.ReadAllAsync(cancellationToken);
}
