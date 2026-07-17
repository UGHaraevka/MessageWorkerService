using MessageWorkerService.Models;

namespace MessageWorkerService.Interfaces;

/// <summary>
/// Контракт сервиса рассылки сообщений.
/// </summary>
public interface IMessageSender
{
    /// <summary>
    /// Отправка сообщения (асинхронная).
    /// </summary>
    /// <param name="payload"> Сообщение на отправку. </param>
    /// <param name="ct"> Токен отмены. </param>
    /// <returns></returns>
    public Task<SenderResultModel> SendMessageAsync(MessagePayload payload, CancellationToken ct);
}