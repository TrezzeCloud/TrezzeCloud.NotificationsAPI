using MassTransit;
using Scalar.AspNetCore;
using TrezzeCloud.Notifications.Application.Consumers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();

builder.Services.AddMassTransit(config =>
{
    config.AddConsumer<UserCreatedConsumer>();
    config.AddConsumer<PaymentProcessedConsumer>();

    config.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("localhost", "/", host =>
        {
            host.Username(
            builder.Configuration["RabbitMq:Username"]!);

            host.Password(
            builder.Configuration["RabbitMq:Password"]!);
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

    config.AddConsumer<PaymentProcessedConsumer>();



});

var app = builder.Build();

app.UseSwagger();

app.UseSwaggerUI();

app.MapControllers();

app.MapScalarApiReference();

app.Run();