FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ActivityMonitor_SyncServer.sln ./
COPY src/SyncServer.Api/SyncServer.Api.csproj ./src/SyncServer.Api/
COPY src/SyncServer.Core/SyncServer.Core.csproj ./src/SyncServer.Core/
COPY src/SyncServer.Infrastructure/SyncServer.Infrastructure.csproj ./src/SyncServer.Infrastructure/

RUN dotnet restore ./src/SyncServer.Api/SyncServer.Api.csproj

COPY src/ ./src/

RUN dotnet publish src/SyncServer.Api/SyncServer.Api.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

RUN apt-get update && apt-get install -y --no-install-recommends \
    curl \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "SyncServer.Api.dll"]