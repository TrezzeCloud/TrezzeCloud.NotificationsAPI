using MassTransit;
using TrezzeCloud.Contracts.Events;

namespace TrezzeCloud.Notifications.Application.Consumers;

public sealed class PaymentProcessedConsumer
    : IConsumer<PaymentProcessedEvent>
{
    public Task Consume(
        ConsumeContext<PaymentProcessedEvent> context)
    {
        if (context.Message.Status != "Approved")
            return Task.CompletedTask;

        Console.WriteLine("====================================");
        Console.WriteLine("PURCHASE CONFIRMATION EMAIL");
        Console.WriteLine($"User: {context.Message.UserId}");
        Console.WriteLine($"Game: {context.Message.GameId}");
        Console.WriteLine($"Price: {context.Message.Price}");
        Console.WriteLine("Purchase approved!");
        Console.WriteLine("====================================");

        return Task.CompletedTask;
    }
}