using MessageWorkerService.Constants;
using MessageWorkerService.Interfaces;
using MessageWorkerService.Models;
using MessageWorkerService.Options;
using Microsoft.Extensions.Options;

namespace MessageWorkerService.Workers;

/// <summary>
/// Воркер.
/// </summary>
/// <param name="logger"> Логгер. </param>
/// <param name="sender"> Сервис отправитель сообщений. </param>
/// <param name="options"> Опции получателя сообщений. </param>
public class Worker(ILogger<Worker> logger, IMessageSender sender, IOptions<MessageReceiverOptions> options)
    : BackgroundService
{
    private readonly MessageReceiverOptions _options = options.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation(LogMessageConstants.WorkerStartedTemplate, _options.TargetUrl, _options.Interval);

        using var timer = new PeriodicTimer(_options.Interval);

        await SendAsync(stoppingToken);

        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                await SendAsync(stoppingToken);
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            logger.LogInformation(LogMessageConstants.WorkerCancelled);
        }
    }

    private Task SendAsync(CancellationToken ct)
    {
        var payload = MessagePayload.Create(_options.MessagePrefix, DateTime.Now);
        return sender.SendMessageAsync(payload, ct);
    }
}