FROM mcr.microsoft.com/dotnet/runtime:10.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["RabbitMQProducer/RabbitMQProducer.csproj", "RabbitMQProducer/"]
RUN dotnet restore "RabbitMQProducer/RabbitMQProducer.csproj"
COPY . .
WORKDIR "/src/RabbitMQProducer"
RUN dotnet build "RabbitMQProducer.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "RabbitMQProducer.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RabbitMQProducer.dll"]
