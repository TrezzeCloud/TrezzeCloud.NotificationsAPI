# TrezzeCloud.NotificationsAPI — legado

Este host ASP.NET Core foi substituído na Fase 3 pelas Azure Functions em repositório próprio. **Não deve mais ser implantado**, nem executado junto às Functions nas mesmas filas RabbitMQ: os consumidores disputariam as mensagens.

Novo repositório: [TrezzeCloud.Notifications.Functions](https://github.com/TrezzeCloud/TrezzeCloud.Notifications.Functions).

No workspace local, a implementação atual fica em `../TrezzeCloud.Notifications.Functions`, com solução independente, Dockerfile, 33 testes e infraestrutura Bicep em `infra/azure`. O endereço remoto acima é o destino previsto; esta migração não cria nem publica o repositório no GitHub.

Permanecem aqui apenas o código legado `Notifications.Api`, `Notifications.Application` e suas cópias de `TrezzeCloud.Contracts`. Os contratos são necessários aos consumidores antigos. A solução foi ajustada para permitir compilar o legado; o Dockerfile da raiz foi preservado somente como registro histórico, não como estratégia de implantação.

O projeto Functions, seus testes e fixture foram transferidos somente após restore, build, 33 testes, build Docker e descoberta dos dois triggers pelo host no novo repositório. O histórico Git e as branches main foram preservados. Nenhum commit ou push foi feito.

<details>
<summary>Documentação anterior preservada para consulta histórica — não seguir como procedimento de implantação</summary>

# TrezzeCloud.NotificationsAPI

Notificações por eventos usando Azure Functions v4, .NET 10 no modelo isolated worker e RabbitMQTrigger. O envio de e-mail é simulado por mensagens no logger; não existe integração SMTP ou envio real.

## Functions e filas

| Function | Evento | Fila | Comportamento |
|---|---|---|---|
| `UserCreatedNotification` | `UserCreatedEvent` | `notifications-user-created` | Simula e-mail de boas-vindas com nome e e-mail |
| `PaymentProcessedNotification` | `PaymentProcessedEvent` | `notifications-payment-processed` | Simula confirmação de compra somente quando `Status` é exatamente `Approved` |

Não há endpoints HTTP de negócio. As Functions são executadas quando o RabbitMQTrigger recebe mensagens das respectivas filas. Ambos os triggers usam `ConnectionStringSetting = "RabbitMqConnection"`.

Os produtores publicam envelopes JSON do MassTransit. `MassTransitEnvelopeDeserializer` extrai o objeto `message` e desserializa o contrato com nomes de propriedades sem distinção entre maiúsculas e minúsculas. Todos os parâmetros obrigatórios dos contratos devem estar presentes; tipos inválidos e valores nulos para strings não anuláveis são rejeitados. Os metadados do envelope são ignorados; o contrato é escolhido pela Function/fila, não por `messageType`.

Exemplo de envelope de pagamento:

```json
{
  "messageId": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
  "messageType": ["urn:message:TrezzeCloud.Contracts.Events:PaymentProcessedEvent"],
  "message": {
    "orderId": "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb",
    "userId": "cccccccc-cccc-cccc-cccc-cccccccccccc",
    "gameId": "dddddddd-dddd-dddd-dddd-dddddddddddd",
    "price": 99.9,
    "status": "Approved",
    "processedAt": "2026-09-05T12:00:00Z"
  }
}
```

Para usuários, `message` contém `userId`, `name`, `email` e `createdAt`.

JSON malformado, raiz inválida, ausência de `message`, `message` nulo ou evento incompatível geram um aviso e retorno normal, sem simular notificação e sem propagar a falha de desserialização. Isso trata o payload como processado: não há retry nem encaminhamento para fila de erro implementado para esses casos. O payload bruto não é registrado. Pagamentos não aprovados também retornam normalmente, sem confirmação de compra.

## Topologia RabbitMQ

No virtual host `/`, as filas devem estar vinculadas aos exchanges `fanout` dos eventos:

| Exchange | Fila de destino |
|---|---|
| `TrezzeCloud.Contracts.Events:UserCreatedEvent` | `notifications-user-created` |
| `TrezzeCloud.Contracts.Events:PaymentProcessedEvent` | `notifications-payment-processed` |

O repositório local `TrezzeCloud.Orchestration` contém essa topologia em `k8s/rabbitmq/definitions.json`, com exchanges e filas duráveis. Provisione/importe essas definições no broker antes de testar a publicação; o atributo RabbitMQTrigger não configura os bindings de publicação do MassTransit.

## Configuração e execução local

Pré-requisitos: SDK .NET 10, Azure Functions Core Tools v4 compatível com o worker do projeto, RabbitMQ com a topologia acima e Azurite (ou uma conta Azure Storage).

| Variável | Valor / finalidade |
|---|---|
| `FUNCTIONS_WORKER_RUNTIME` | `dotnet-isolated` |
| `RabbitMqConnection` | URI AMQP, por exemplo `amqp://guest:guest@localhost:5672/` em ambiente local |
| `AzureWebJobsStorage` | `UseDevelopmentStorage=true` com Azurite local, ou connection string de Storage |

As variáveis `RabbitMq__Host`, `RabbitMq__Username` e `RabbitMq__Password` pertencem à API antiga; as Functions usam a URI única `RabbitMqConnection`.

Crie `src/TrezzeCloud.Notifications.Functions/local.settings.json` (ignorado pelo Git) com valores locais:

```json
{
  "IsEncrypted": false,
  "Values": {
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
    "RabbitMqConnection": "amqp://guest:guest@localhost:5672/",
    "AzureWebJobsStorage": "UseDevelopmentStorage=true"
  }
}
```

Não versione credenciais. Em containers, use os nomes de serviço e endpoints de Storage acessíveis pela rede do container em vez de `localhost`.

Na raiz do repositório:

```powershell
dotnet restore TrezzeCloud.NotificationsAPI.slnx
dotnet build TrezzeCloud.NotificationsAPI.slnx --no-restore
dotnet test TrezzeCloud.NotificationsAPI.slnx --no-build --no-restore
Set-Location src/TrezzeCloud.Notifications.Functions
func start
```

Com RabbitMQ e Azurite ativos, o host deve listar `UserCreatedNotification` e `PaymentProcessedNotification`. Publique os eventos pela UsersAPI/PaymentsAPI ou envelopes de teste nos exchanges correspondentes. O resultado aparece no console do host. Apenas `dotnet run` não substitui o host do Core Tools para ativar os triggers.

Os testes unitários executam as duas Functions diretamente com logger em memória. Cobrem os dois contratos, camelCase/PascalCase, preservação de campos, confirmação de pagamento aprovado, pagamentos não aprovados e payloads inválidos. Não exigem RabbitMQ ou Storage e não verificam entrega/ack pelo host real.

## Docker

Use o Dockerfile das Functions com o contexto na raiz deste repositório:

```powershell
docker build -f src/TrezzeCloud.Notifications.Functions/Dockerfile -t trezzecloud-notifications-functions .
```

O Docker Compose existente em `TrezzeCloud.Orchestration` já aponta para esse Dockerfile e configura RabbitMQ e Azurite.

## Projeto legado Notifications.Api

`src/TrezzeCloud.Notifications.Api` foi preservado. Ele é um host ASP.NET Core com consumidores MassTransit e não é necessário para executar as Functions. Já está fora da solução `.slnx`; a biblioteca `Notifications.Application`, com os consumidores antigos, ainda está na solução.

Há dependências legadas: o Dockerfile da raiz ainda publica `Notifications.Api`, e o manifesto `k8s/notifications-api/notifications-api.yaml` no repositório de orquestração usa configurações da API antiga. Remover o projeto agora quebraria o build Docker da raiz e exigiria revisar a implantação Kubernetes e as referências à biblioteca Application. Uma remoção futura deve migrar esses caminhos primeiro.

Não execute a API antiga e as Functions simultaneamente nas mesmas filas: elas competiriam pelas mensagens, dividindo o processamento entre os dois hosts. O funcionamento serverless não depende de iniciar a API antiga para criar a topologia; use as definições explícitas do RabbitMQ.

A configuração de telemetria já existente no projeto foi preservada; nenhuma stack de observabilidade foi implementada nesta etapa.

</details>
