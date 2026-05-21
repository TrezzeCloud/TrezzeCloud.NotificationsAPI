# README - NotificationsAPI

```md
# TrezzeCloud.NotificationsAPI

Microsserviço responsável pelo envio de notificações.

## Responsabilidades

- Consumo de UserCreatedEvent
- Consumo de PaymentProcessedEvent
- Simulação de envio de e-mails

---

# Tecnologias

- .NET 10
- ASP.NET Core
- RabbitMQ
- MassTransit
- Docker
- Kubernetes

---

# Variáveis de Ambiente

| Variável | Descrição |
|---|---|
| RabbitMq__Host | Host RabbitMQ |
| RabbitMq__Username | Usuário RabbitMQ |
| RabbitMq__Password | Senha RabbitMQ |

---

# Executar Localmente

```bash
dotnet restore
dotnet run
````

---

# Docker

```bash
docker build -t trezzecloud-notifications-api .
```

---

# Kubernetes

Manifestos disponíveis em:

```txt
k8s/notifications-api
```

```
```
