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

# Phase 4D.1 — pre-create the document storage path AND the log directory
# with appuser ownership so the FIRST mount of an empty named volume inherits
# it and the Serilog file sink can actually write/rotate.
RUN useradd --create-home --shell /bin/bash appuser \
    && mkdir -p /data/documents /app/Logs \
    && chown -R appuser:appuser /data /app/Logs

# Phase 4D.1 — the base image ships neither wget nor curl; the container
# healthcheck needs one to probe /health endpoints.
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

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
HEALTHCHECK --interval=15s --timeout=5s --start-period=40s --retries=5 \
    CMD curl -fsS http://localhost:8080/health/ready || exit 1

ENTRYPOINT ["dotnet", "LaoHR.API.dll"]
