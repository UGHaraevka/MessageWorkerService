using MessageWorkerService.Constants;
using Polly;
using Polly.Extensions.Http;

namespace MessageWorkerService.Policies;

/// <summary>
/// Фабрика политики ретраев.
/// </summary>
public static class RetryPolicyFactory
{
    /// <summary>
    /// Создает политику ретраев с экспоненциальным ожиданием и попыткой до 3 раз.
    /// </summary>
    /// <param name="logger"> Экземпляр логгера. </param>
    /// <returns>  </returns>
    public static IAsyncPolicy<HttpResponseMessage> Create(ILogger logger)
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                onRetry: (outcome, delay, attempt, _) =>
                {
                    var reason = outcome.Exception is not null
                        ? outcome.Exception.Message
                        : $"HTTP {(int)outcome.Result.StatusCode}";
 
                    logger.LogWarning(LogMessageConstants.RetryTemplate, attempt, reason, delay.TotalSeconds);
                });
    }
}