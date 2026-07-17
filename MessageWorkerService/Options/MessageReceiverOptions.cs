using System.ComponentModel.DataAnnotations;

namespace MessageWorkerService.Options;

/// <summary>
/// Настройки получателя сообщений.
/// </summary>
public class MessageReceiverOptions
{
    /// <summary>
    /// Адрес получателя сообщений.
    /// </summary>
    [Required]
    public string TargetUrl { get; init; }

    /// <summary>
    /// Префикс сообщения.
    /// </summary>
    [Required]
    public string MessagePrefix { get; init; }

    /// <summary>
    /// Таймаут запросов.
    /// </summary>
    [Range(typeof(TimeSpan), "00:00:01", "00:05:00")]
    public TimeSpan RequestTimeout { get; init; }

    /// <summary>
    /// Интервал между запросами.
    /// </summary>
    [Range(typeof(TimeSpan), "00:00:01", "24:00:00")]
    public TimeSpan Interval { get; init; }
}