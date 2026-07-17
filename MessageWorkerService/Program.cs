using MessageWorkerService.Configuration;
using MessageWorkerService.Constants;
using MessageWorkerService.Interfaces;
using Microsoft.Extensions.Options;
using MessageWorkerService.Options;
using MessageWorkerService.Policies;
using MessageWorkerService.Services;
using MessageWorkerService.Workers;

var builder = Host.CreateApplicationBuilder(args);

builder.Logging.AddSimpleLogging();

var services = builder.Services;
var configuration = builder.Configuration;

services.AddMessageReceiverOptionsAndValidate(configuration);

services.AddHttpClient(ClientConstants.ClientName, (sp, client) =>
    {
        var options = sp.GetRequiredService<IOptions<MessageReceiverOptions>>().Value;
        client.Timeout = options.RequestTimeout;
    })
    .AddPolicyHandler((sp, _) => RetryPolicyFactory.Create(sp.GetRequiredService<ILogger<MessageSender>>()));

services.AddSingleton<IMessageSender, MessageSender>();
services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();