using System.ComponentModel.DataAnnotations;
using MessageWorkerService.Constants;
using MessageWorkerService.Options;

namespace MessageWorkerService.Configuration;

/// <summary>
/// Расширения для регистрации сервисов.
/// </summary>
public static class ServicesExtensions
{
    /// <summary>
    /// Добавляет простое логгирование в консоль с датой и временем.
    /// </summary>
    /// <param name="loggingBuilder"> Билдер логов. </param>
    public static void AddSimpleLogging(this ILoggingBuilder loggingBuilder)
    {
        loggingBuilder.ClearProviders();
        loggingBuilder.AddSimpleConsole(options =>
        {
            options.TimestampFormat = "yyyy-MM-dd HH:mm:ss.fff ";
            options.SingleLine = true;
        });
    }

    /// <summary>
    /// Добавляет настройки получателя сообщений и валидирует их при регистрации.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    public static void AddMessageReceiverOptionsAndValidate(this IServiceCollection services, IConfiguration configuration)
    {
        var section = configuration.GetSection(nameof(MessageReceiverOptions));

        services.AddOptions<MessageReceiverOptions>()
            .Bind(section)
            .Validate(options =>
            {
                var context = new ValidationContext(options);
                return Validator.TryValidateObject(options, context, null, validateAllProperties: true);
            }, LogMessageConstants.MessageReceiverOptionsFieldsAreEmpty)
            .Validate(_ => section.Exists(),
                LogMessageConstants.MessageReceiverOptionsIsEmpty)
            .ValidateOnStart();
    }
}