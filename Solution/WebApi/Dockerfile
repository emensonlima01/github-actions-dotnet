# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
ARG BUILD_CONFIGURATION=Release
ARG VERSION=1.0.0
WORKDIR /src

# Copy project files and restore dependencies (cached layer)
COPY ["Domain/Domain.csproj", "Domain/"]
COPY ["Application/Application.csproj", "Application/"]
COPY ["Infrastructure/Infrastructure.csproj", "Infrastructure/"]
COPY ["IoC/IoC.csproj", "IoC/"]
COPY ["WebApi/WebApi.csproj", "WebApi/"]

# Restore dependencies
RUN dotnet restore "WebApi/WebApi.csproj"

# Copy source code
COPY . .

# Publish optimized build
WORKDIR /src/WebApi
RUN dotnet publish "WebApi.csproj" \
    -c $BUILD_CONFIGURATION \
    -o /app/publish \
    --no-restore \
    --self-contained false \
    /p:DebugType=None \
    /p:DebugSymbols=false

# Final runtime image - using minimal aspnet Alpine image
FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS final
ARG VERSION=1.0.0

# Add metadata labels
LABEL org.opencontainers.image.title="WebApi Payment Service" \
      org.opencontainers.image.description="Payment API with Clean Architecture" \
      org.opencontainers.image.version="${VERSION}" \
      org.opencontainers.image.vendor="Payment Service"

WORKDIR /app

# Install curl for health check (lightweight alternative to wget)
RUN apk add --no-cache curl

# Create non-root user for security
RUN addgroup -g 1000 appgroup && \
    adduser -u 1000 -G appgroup -s /bin/sh -D appuser && \
    chown -R appuser:appgroup /app

USER appuser

# Copy only published files
COPY --from=build --chown=appuser:appgroup /app/publish .

# Expose ports
EXPOSE 8080

# Health check using curl (lighter than wget)
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

# Set environment variables for optimization
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=true \
    DOTNET_EnableDiagnostics=0 \
    ASPNETCORE_HTTP_PORTS=8080

ENTRYPOINT ["dotnet", "WebApi.dll"]