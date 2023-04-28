FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["RabbitMQConsumer/RabbitMQConsumer.csproj", "RabbitMQConsumer/"]
RUN dotnet restore "RabbitMQConsumer/RabbitMQConsumer.csproj"
COPY . .
WORKDIR "/src/RabbitMQConsumer"
RUN dotnet build "RabbitMQConsumer.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "RabbitMQConsumer.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RabbitMQConsumer.dll"]
