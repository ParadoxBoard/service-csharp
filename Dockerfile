# ══════════════════════════════════════════════════════════════════════════════
# Dockerfile - .NET/C# Backend
# ══════════════════════════════════════════════════════════════════════════════

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY . .
RUN dotnet restore
RUN dotnet publish -c Release -o /app

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app .

# Puerto (cambiar segun tu app)
EXPOSE 8080

# Etiqueta para GHCR (CONFIGURAR)
LABEL org.opencontainers.image.source="https://github.com/ParadoxBoard/service-csharp.git"

# CONFIGURAR: Cambiar por el nombre de tu DLL
ENTRYPOINT ["dotnet", "service-csharp.dll"]
