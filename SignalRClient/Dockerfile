FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["SignalRClient/SignalRClient.csproj", "SignalRClient/"]
RUN dotnet restore "SignalRClient/SignalRClient.csproj"
COPY . .
WORKDIR "/src/SignalRClient"
RUN dotnet build "SignalRClient.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SignalRClient.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SignalRClient.dll"]
