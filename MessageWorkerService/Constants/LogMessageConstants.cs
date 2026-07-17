namespace MessageWorkerService.Constants;

/// <summary>
/// Сообщения для логов.
/// </summary>
public static class LogMessageConstants
{
    public const string MessageSendSuccessfulTemplate = "Сообщение {MessageId} успешно отправлено. Статус: {StatusCode}";
    public const string MessageSendUnsuccessfulTemplate = "Сообщение {MessageId} не отправлено. Статус: {StatusCode}. Ответ: {Body}";
    public const string RetryTemplate = "Попытка {Attempt}/3 отправки не удалась ({Reason}). Повтор через {Delay}с";
    public const string WorkerStartedTemplate = "Worker запущен. Целевой адрес: {TargetUrl}, интервал: {Interval}";
    public const string WorkerCancelled = "Worker останавливается по запросу отмены";
    public const string ErrorOnSendMessageTemplate = "Ошибка при отправке сообщения {MessageId}";
    public const string MessageReceiverOptionsFieldsAreEmpty = "MessageReceiverOptions: не заполнены обязательные поля";
    public const string MessageReceiverOptionsIsEmpty = "Секция конфигурации 'MessageReceiverOptions' отсутствует в appsettings.json";
}