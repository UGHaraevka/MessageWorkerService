namespace MessageWorkerService.Models;

/// <summary>
/// Результирующая модель отправки сообщений.
/// </summary>
public class SenderResultModel(bool isSuccess, string message, string? error = null)
{
    /// <summary>
    /// Успешность.
    /// </summary>
    public bool IsSuccess { get; set; } = isSuccess;

    /// <summary>
    /// Сообщение.
    /// </summary>
    public string Message { get; set; } = message;

    /// <summary>
    /// Возникшая ошибка.
    /// </summary>
    public string? Error { get; set; } = error;
}