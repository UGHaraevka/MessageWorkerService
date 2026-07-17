using System.Text.Json.Serialization;

namespace MessageWorkerService.Models;

/// <summary>
/// Тело POST-запроса, отправляемое на целевой адрес получателя сообщений.
/// </summary>
public sealed class MessagePayload
{
    [JsonPropertyName("id")]
    public Guid Id { get; init; }
 
    /// <summary>
    /// Время отправки.
    /// </summary>
    [JsonPropertyName("timesend")]
    public string TimeSend { get; init; } = string.Empty;
    
    /// <summary>
    /// Сообщение.
    /// </summary>
    [JsonPropertyName("msg")]
    public string Message { get; init; } = string.Empty;
 
    /// <summary>
    /// Создание сообщения на отправку.
    /// </summary>
    /// <param name="fixedMessagePrefix"> Префикс в сообщении. </param>
    /// <param name="sentAt"> Время предполагаемой отправки. </param>
    /// <returns></returns>
    public static MessagePayload Create(string fixedMessagePrefix, DateTime sentAt)
    {
        return new MessagePayload
        {
            Id = Guid.NewGuid(),
            TimeSend = sentAt.ToString("yyyy-MM-ddTHH:mm:ss.fff"),
            Message = $"{fixedMessagePrefix}_{sentAt:yyyy-MM-dd}"
        };
    }
}