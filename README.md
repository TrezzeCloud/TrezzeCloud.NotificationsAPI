# TrezzeCloud.NotificationsAPI — legado

Este host ASP.NET Core foi substituído na Fase 3 pelas Azure Functions em repositório próprio. **Não deve mais ser implantado**, nem executado junto às Functions nas mesmas filas RabbitMQ: os consumidores disputariam as mensagens.

A implementação serverless foi publicada no repositório: [TrezzeCloud.Notifications.Functions](https://github.com/TrezzeCloud/TrezzeCloud.Notifications.Functions).

No workspace local, a implementação atual fica em `../TrezzeCloud.Notifications.Functions`, com solução independente, Dockerfile, 33 testes e infraestrutura Bicep em `infra/azure`.

Permanecem aqui apenas o código legado `Notifications.Api`, `Notifications.Application` e suas cópias de `TrezzeCloud.Contracts`. Os contratos são necessários aos consumidores antigos. A solução foi ajustada para permitir compilar o legado; o Dockerfile da raiz foi preservado somente como registro histórico, não como estratégia de implantação.

O projeto Functions, seus testes e fixture foram transferidos somente após restore, build, 33 testes, build Docker e descoberta dos dois triggers pelo host no novo repositório. O histórico Git e as branches main foram preservados.

A documentação operacional anterior pode ser consultada no histórico Git. Para executar ou implantar as Functions, use exclusivamente a documentação do novo repositório.
