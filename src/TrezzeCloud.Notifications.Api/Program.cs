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

    config.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("localhost", "/", host =>
        {
            host.Username("guest");
            host.Password("guest");
        });

        cfg.ReceiveEndpoint("notifications-user-created", endpoint =>
        {
            endpoint.ConfigureConsumer<UserCreatedConsumer>(context);
        });
    });
});

var app = builder.Build();

app.UseSwagger();

app.UseSwaggerUI();

app.MapControllers();

app.MapScalarApiReference();

app.Run();