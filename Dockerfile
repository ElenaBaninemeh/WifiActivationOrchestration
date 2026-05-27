FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/WifiActivationOrchestration.Api/WifiActivationOrchestration.Api.csproj src/WifiActivationOrchestration.Api/
RUN dotnet restore src/WifiActivationOrchestration.Api/WifiActivationOrchestration.Api.csproj

COPY . .
RUN dotnet publish src/WifiActivationOrchestration.Api/WifiActivationOrchestration.Api.csproj \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "WifiActivationOrchestration.Api.dll"]