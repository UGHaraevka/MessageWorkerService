
# MessageWorkerService
Сервис по отправке сообщений определенного формата с периодичностью на указанный в конфигах адрес.

```MessageWorkerService/
├── Configuration/
│   └── ServicesExtensions.cs        — extension-методы регистрации DI
├── Constants/
│   ├── ClientConstants.cs           — константы для работы с http клиентом
│   └── LogMessageConstants.cs       — шаблоны сообщений для логов
├── Interfaces/
│   └── IMessageSender.cs            — контракт отправки одного сообщения
├── Models/
│   ├── MessagePayload.cs            — тело запроса сообщения
│   └── SenderResultModel.cs         — результирующая модель отправки сообщения
├── Options/
│   └── MessageReceiverOptions.cs    — настройки из appsettings.json (адрес, интервал, таймаут, префикс)
├── Policies/
│   └── RetryPolicyFactory.cs        — retry-политика Polly (переиспользуется в тестах)
├── Services/
│   └── MessageSender.cs             — реализация отправки: POST + логирование результата
├── Workers/
│   └── Worker.cs                    — BackgroundService: планирование по PeriodicTimer
├── appsettings.json                 — конфигурация
├── appsettings.Development.json     — конфигурация dev среды
├── Dockerfile
└── Program.cs                       — точка входа

MessageWorkerService.IntegrationTests/
├── StubHttpMessageHandler.cs        — заглушка транспорта HttpClient ("мок-сервер" без реальной сети)
└── WorkerWithTestHostTest.cs        — интеграционные тесты (Worker/MessageSender + retry-логика)
```

Используемые пакеты:

| Пакет     | Назначение                                                              | Бесплатно? |
|:----------|:------------------------------------------------------------------------|:-----------|
| `Microsoft.Extensions.Hosting` | Host.CreateApplicationBuilder, DI-контейнер, конфигурация, логирование. | Да         |
| `Microsoft.Extensions.Http` | Регистрация IHttpClientFactory в DI.                                    | Да         |
| `Microsoft.Extensions.Http.Polly` | Политика ретраев.                                                       | Да         |
| `Polly.Extensions.Http` | Готовый набор правил для работы с ретраями.                             | Да         |

Для проекта с интеграционными тестами (MessageWorkerService.IntegrationTests) используется xunit с так же бесплатной лицензией.

## Обработка неуспешной отправки

Неуспех запроса retry-политикой Polly на уровне `HttpClient` (см. `Policies/RetryPolicyFactory.cs`):

- до 3 повторных попыток с экспоненциальной задержкой между ними
- каждая неудачная попытка логируется как `Warning` с указанием причины и временем до следующей попытки
- если все попытки исчерпаны — итоговый запрос логируется как `Error`, сервис продолжает работу, следующая отправка произойдёт через время указанное в `Interval` (по умолчанию 2 минуты)

Ответы вида 4xx (кроме 408) — например, 400 Bad Request — не ретраятся, повтор не изменит результат.

## Запуск:
- `cd MessageWorkerService`
- `dotnet run`

## Запуск тестов:
- `cd MessageWorkerService.IntegrationTests`
- `dotnet test`

## Пример appsettings.json

```{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.Hosting.Lifetime": "Information",
      "System.Net.Http.HttpClient": "Warning"
    }
  },
  "MessageReceiverOptions": {
    "TargetUrl": "https://example.com/api/receive",
    "MessagePrefix": "test_message",
    "Interval": "00:02:00",
    "RequestTimeout": "00:00:30"
  }
}
```

# Постановка:
Реализовать на .net простой микросервис (на базе SDK Worker, например), который выполняет следующие действия.
- Каждые 2 минуты выполняет POST-запрос на указанный web-адрес, передавая указанное сообщение.
  Успех или не успех должны логироваться в стандартный вывод (консоль)
  Исходные данные:
  Формат передаваемого сообщения:
  JSON
  {
  "id": "a828d11f-e898-4830-a426-160004e0e1e7",
  "timesend": "2026-03-05T17:03:22.123",
  "msg": "test_message_2026-03-05"
  }

описание полей:
id - идентификатор сообщения, формат GUID
timesend: время отправки сообщения
msg - сообщение для отправки. Формируется из фиксированного сообщения и даты отправки

Разрешено использовать только бесплатные сторонние nuget-пакеты (с обоснованием их необходимости)

