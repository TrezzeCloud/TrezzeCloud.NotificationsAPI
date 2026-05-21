FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY . .

RUN dotnet restore src/TrezzeCloud.Notifications.Api/TrezzeCloud.Notifications.Api.csproj

RUN dotnet publish src/TrezzeCloud.Notifications.Api/TrezzeCloud.Notifications.Api.csproj \
    -c Release \
    -o /app/publish

FROM base AS final
WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "TrezzeCloud.Notifications.Api.dll"]