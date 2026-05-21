using MassTransit;
using Scalar.AspNetCore;
using TrezzeCloud.Notifications.Application.Consumers;

var builder = WebApplication.CreateBuilder(args);
var rabbitHost = builder.Configuration["RabbitMq:Host"] ?? "rabbitmq";
var rabbitUsername = builder.Configuration["RabbitMq:Username"] ?? "guest";
var rabbitPassword = builder.Configuration["RabbitMq:Password"] ?? "guest";

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();

builder.Services.AddMassTransit(config =>
{
    config.AddConsumer<UserCreatedConsumer>();
    config.AddConsumer<PaymentProcessedConsumer>();

    config.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(
            rabbitHost,
            "/",
            host =>
            {
                host.Username(rabbitUsername);
                host.Password(rabbitPassword);
            });

        cfg.ReceiveEndpoint("notifications-user-created", endpoint =>
        {
            endpoint.ConfigureConsumer<UserCreatedConsumer>(context);
        });

        cfg.ReceiveEndpoint("notifications-payment-processed", endpoint =>
        {
            endpoint.ConfigureConsumer<PaymentProcessedConsumer>(context);
        });

    });

});

var app = builder.Build();

app.UseSwagger();

app.UseSwaggerUI();

app.MapControllers();

app.MapScalarApiReference();

app.Run();