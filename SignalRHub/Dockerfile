FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["SignalRHub/SignalRHub.csproj", "SignalRHub/"]
RUN dotnet restore "SignalRHub/SignalRHub.csproj"
COPY . .
WORKDIR "/src/SignalRHub"
RUN dotnet build "SignalRHub.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SignalRHub.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SignalRHub.dll"]
