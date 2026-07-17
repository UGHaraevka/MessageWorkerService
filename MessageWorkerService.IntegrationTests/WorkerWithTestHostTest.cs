using System.Net;
using MessageWorkerService.Models;
using MessageWorkerService.Options;
using MessageWorkerService.Policies;
using Xunit;
using MessageWorkerService.Services;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace MessageWorkerService.IntegrationTests;

public class WorkerWithTestHostTest
{
    private static IOptions<MessageReceiverOptions> _options = Microsoft.Extensions.Options.Options.Create(
        new MessageReceiverOptions
        {
            TargetUrl = "http://localhost/api/receive",
            MessagePrefix = "test_message",
            RequestTimeout = TimeSpan.FromSeconds(5)
        });

    private class SingleClientFactory(HttpClient client) : IHttpClientFactory
    {
        public HttpClient CreateClient(string name) => client;
    }

    private static (MessageSender Sender, StubHttpMessageHandler Handler) CreateSender(
        params HttpStatusCode[] statusSequence)
    {
        var handler = new StubHttpMessageHandler(statusSequence);
        var policy = RetryPolicyFactory.Create(NullLogger.Instance);
        var policyHandler = new PolicyHttpMessageHandler(policy) { InnerHandler = handler };

        var httpClient = new HttpClient(policyHandler);

        var sender = new MessageSender(
            new SingleClientFactory(httpClient),
            NullLogger<MessageSender>.Instance,
            _options);

        return (sender, handler);
    }

    [Fact]
    public async Task SendAsync_SuccessOnFirstAttempt_ReturnsTrueAndDoesNotRetry()
    {
        // Arrange
        var (sender, handler) = CreateSender(HttpStatusCode.OK);
        var payload = MessagePayload.Create(_options.Value.MessagePrefix, DateTime.Now);

        // Act
        var result = await sender.SendMessageAsync(payload, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(1, handler.CallCount);
    }

    [Fact]
    public async Task SendAsync_FailsTwiceThenSucceeds_RetriesAndEventuallyReturnsTrue()
    {
        // Arrange
        var (sender, handler) = CreateSender(
            HttpStatusCode.InternalServerError,
            HttpStatusCode.InternalServerError,
            HttpStatusCode.OK);

        var payload = MessagePayload.Create(_options.Value.MessagePrefix, DateTime.Now);

        // Act
        var result = await sender.SendMessageAsync(payload, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(3, handler.CallCount);
    }

    [Fact]
    public async Task SendAsync_AlwaysFails_ReturnsFalseAfterExhaustingAllRetriesWithoutThrowing()
    {
        // Arrange
        var (sender, handler) = CreateSender(HttpStatusCode.InternalServerError);
        var payload = MessagePayload.Create(_options.Value.MessagePrefix, DateTime.Now);

        // Act
        var result = await sender.SendMessageAsync(payload, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(4, handler.CallCount);
    }

    [Fact]
    public async Task SendAsync_NonTransientClientError_DoesNotRetry()
    {
        // Arrange
        var (sender, handler) = CreateSender(HttpStatusCode.BadRequest);
        var payload = MessagePayload.Create(_options.Value.MessagePrefix, DateTime.Now);

        // Act
        var result = await sender.SendMessageAsync(payload, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(1, handler.CallCount);
    }

    [Fact]
    public async Task SendAsync_SendsPayloadMatchingSpecFormat()
    {
        // Arrange
        var (sender, handler) = CreateSender(HttpStatusCode.OK);
        var payload = MessagePayload.Create(_options.Value.MessagePrefix, DateTime.Now);

        // Act
        await sender.SendMessageAsync(payload, CancellationToken.None);

        // Assert
        Assert.Single(handler.RequestBodies);
        var body = handler.RequestBodies[0];
        Assert.Contains("\"id\"", body);
        Assert.Contains("\"timesend\"", body);
        Assert.Contains("\"msg\"", body);
        Assert.Contains("test_message_", body);
    }
}