using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using TrezzeCloud.Contracts.Events;

namespace TrezzeCloud_Notifications_Functions.Functions;

public class PaymentProcessedFunction
{
    private readonly ILogger<PaymentProcessedFunction> _logger;

    public PaymentProcessedFunction(
        ILogger<PaymentProcessedFunction> logger)
    {
        _logger = logger;
    }

    [Function("PaymentProcessedNotification")]
    public Task Run(
        [RabbitMQTrigger(
            "notifications-payment-processed",
            ConnectionStringSetting = "RabbitMqConnection")]
        string message)
    {
        _logger.LogInformation(
            "Mensagem recebida da fila notifications-payment-processed.");

        var payment =
            JsonSerializer.Deserialize<PaymentProcessedEvent>(
                message,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

        if (payment is null)
        {
            _logger.LogWarning(
                "Não foi possível desserializar PaymentProcessedEvent.");

            return Task.CompletedTask;
        }

        if (payment.Status != "Approved")
        {
            _logger.LogInformation(
                "Pagamento não aprovado. Nenhuma notificação será enviada.");

            return Task.CompletedTask;
        }

        _logger.LogInformation(
            """
            ====================================
            PURCHASE CONFIRMATION EMAIL
            User: {UserId}
            Game: {GameId}
            Price: {Price}
            Purchase approved!
            ====================================
            """,
            payment.UserId,
            payment.GameId,
            payment.Price);

        return Task.CompletedTask;
    }
}