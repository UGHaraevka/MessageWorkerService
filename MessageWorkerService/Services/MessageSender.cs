using MessageWorkerService.Interfaces;
using MessageWorkerService.Models;
using MessageWorkerService.Options;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using MessageWorkerService.Constants;

namespace MessageWorkerService.Services;

/// <inheritdoc />
public class MessageSender(IHttpClientFactory httpClientFactory, ILogger<MessageSender> logger,
    IOptions<MessageReceiverOptions> options) : IMessageSender
{
    private readonly MessageReceiverOptions _options = options.Value;

    /// <inheritdoc />
    public async Task<SenderResultModel> SendMessageAsync(MessagePayload payload, CancellationToken ct)
    {
        try
        {
            var client = httpClientFactory.CreateClient(ClientConstants.ClientName);

            using var response = await client.PostAsJsonAsync(
                _options.TargetUrl, payload, ct);

            if (response.IsSuccessStatusCode)
            {
                logger.LogInformation(
                    LogMessageConstants.MessageSendSuccessfulTemplate,
                    payload.Id, (int)response.StatusCode);
            }
            else
            {
                var body = await response.Content.ReadAsStringAsync(ct);
                logger.LogWarning(
                    LogMessageConstants.MessageSendUnsuccessfulTemplate,
                    payload.Id, (int)response.StatusCode, body);
                
                return new SenderResultModel(isSuccess: false, payload.Message, error: response.ReasonPhrase);
            }
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, LogMessageConstants.ErrorOnSendMessageTemplate, payload.Id);

            return new SenderResultModel(isSuccess: false, payload.Message, error: ex.Message);
        }
        
        return new SenderResultModel(isSuccess: true, payload.Message);
    }
}