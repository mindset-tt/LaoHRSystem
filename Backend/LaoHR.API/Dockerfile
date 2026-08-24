# Phase 6e — Multi-stage Dockerfile for the LaoHR API.
# Lightweight (no SDK in final image, single runtime base).
# Scalability: the published output is layered so the deps layer is cached
# independently of the application layer.

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files first to maximize Docker layer cache.
COPY LaoHR.API/LaoHR.API.csproj LaoHR.API/
COPY LaoHR.Shared/LaoHR.Shared.csproj LaoHR.Shared/
COPY LaoHR.Tests/LaoHR.Tests.csproj LaoHR.Tests/

# Restore — cached as long as the csproj files don't change.
RUN dotnet restore LaoHR.API/LaoHR.API.csproj

# Copy everything else and publish.
COPY . .
RUN dotnet publish LaoHR.API/LaoHR.API.csproj -c Release -o /app/publish --no-restore

# Runtime — slim base, non-root user, ASPNETCORE_URLS pre-set.
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Create non-root user for the API process.
RUN useradd --create-home --shell /bin/bash appuser

# Copy published output from the build stage.
COPY --from=build /app/publish .

# Runtime defaults — overridable via env vars at deploy time.
ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_RUNNING_IN_CONTAINER=true \
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

EXPOSE 8080

USER appuser

# Health-check uses the readiness endpoint added in Phase 6g.
HEALTHCHECK --interval=30s --timeout=5s --start-period=20s --retries=3 \
    CMD wget -qO- http://localhost:8080/health/live || exit 1

ENTRYPOINT ["dotnet", "LaoHR.API.dll"]
