FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY tools/WifiActivationOrchestration.MockServer/WifiActivationOrchestration.MockServer.csproj tools/WifiActivationOrchestration.MockServer/
RUN dotnet restore tools/WifiActivationOrchestration.MockServer/WifiActivationOrchestration.MockServer.csproj

COPY . .
RUN dotnet publish tools/WifiActivationOrchestration.MockServer/WifiActivationOrchestration.MockServer.csproj \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "WifiActivationOrchestration.MockServer.dll"]