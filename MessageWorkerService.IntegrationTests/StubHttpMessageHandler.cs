using System.Net;

namespace MessageWorkerService.IntegrationTests;

internal sealed class StubHttpMessageHandler(params HttpStatusCode[] statusSequence) : HttpMessageHandler
{
    private int _index;
    public int CallCount { get; private set; }
    public List<string> RequestBodies { get; } = [];
 
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        CallCount++;
 
        if (request.Content is not null)
        {
            RequestBodies.Add(await request.Content.ReadAsStringAsync(cancellationToken));
        }
 
        var status = _index < statusSequence.Length
            ? statusSequence[_index]
            : statusSequence[^1];
 
        _index++;
 
        return new HttpResponseMessage(status);
    }
}