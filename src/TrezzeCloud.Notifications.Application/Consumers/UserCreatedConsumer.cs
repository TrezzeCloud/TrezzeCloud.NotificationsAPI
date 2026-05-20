using MassTransit;
using TrezzeCloud.Contracts.Events;

namespace TrezzeCloud.Notifications.Application.Consumers;

public sealed class UserCreatedConsumer
    : IConsumer<UserCreatedEvent>
{
    public Task Consume(
        ConsumeContext<UserCreatedEvent> context)
    {
        Console.WriteLine("====================================");
        Console.WriteLine("WELCOME EMAIL");
        Console.WriteLine($"To: {context.Message.Email}");
        Console.WriteLine($"User: {context.Message.Name}");
        Console.WriteLine("Welcome to TrezzeCloud!");
        Console.WriteLine("====================================");

        return Task.CompletedTask;
    }
}