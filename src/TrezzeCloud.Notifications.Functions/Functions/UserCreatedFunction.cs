using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using TrezzeCloud.Contracts.Events;

namespace TrezzeCloud_Notifications_Functions.Functions;

public class UserCreatedFunction
{
    private readonly ILogger<UserCreatedFunction> _logger;

    public UserCreatedFunction(
        ILogger<UserCreatedFunction> logger)
    {
        _logger = logger;
    }

    [Function("UserCreatedNotification")]
    public Task Run(
        [RabbitMQTrigger(
            "notifications-user-created",
            ConnectionStringSetting = "RabbitMqConnection")]
        string rawMessage)
    {
        _logger.LogInformation(
            "Mensagem recebida da fila notifications-user-created.");

        using var document = JsonDocument.Parse(rawMessage);

        if (!document.RootElement.TryGetProperty("message", out var messageElement))
        {
            _logger.LogWarning(
                "Mensagem recebida sem envelope 'message' do MassTransit.");

            _logger.LogInformation(
                "Payload recebido: {Payload}",
                rawMessage);

            return Task.CompletedTask;
        }

        var userCreatedEvent =
            messageElement.Deserialize<UserCreatedEvent>(
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

        if (userCreatedEvent is null)
        {
            _logger.LogWarning(
                "Não foi possível desserializar UserCreatedEvent.");

            return Task.CompletedTask;
        }

        _logger.LogInformation(
            """
            ====================================
            WELCOME EMAIL
            To: {Email}
            User: {Name}
            Welcome to TrezzeCloud!
            ====================================
            """,
            userCreatedEvent.Email,
            userCreatedEvent.Name);

        return Task.CompletedTask;
    }
}