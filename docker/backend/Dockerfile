# ============================
# Stage 1 - Base Runtime
# ============================
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base

WORKDIR /app
EXPOSE 8080

USER $APP_UID

# ============================
# Stage 2 - Build
# ============================
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

# Copy solution & props files for restore caching
COPY Directory.Build.props .
COPY Directory.Packages.props .
COPY ByteLink.slnx .

COPY src/backend/Host/ByteLink.Api/ByteLink.Api.csproj src/backend/Host/ByteLink.Api/
COPY src/backend/Shared/Shared.csproj src/backend/Shared/
COPY src/backend/Modules/Identity/Modules.Identity/Modules.Identity.csproj src/backend/Modules/Identity/Modules.Identity/
COPY src/backend/Modules/Identity/Modules.Identity.IntegrationEvents/Modules.Identity.IntegrationEvents.csproj src/backend/Modules/Identity/Modules.Identity.IntegrationEvents/
COPY src/backend/Modules/Notifications/Modules.Notifications/Modules.Notifications.csproj src/backend/Modules/Notifications/Modules.Notifications/

# Cache NuGet packages across builds
RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages \
    dotnet restore src/backend/Host/ByteLink.Api/ByteLink.Api.csproj

COPY . .

RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages \
    dotnet publish src/backend/Host/ByteLink.Api/ByteLink.Api.csproj \
    -c Release \
    -o /app/publish \
    --no-restore \
    /p:UseAppHost=false

# ============================
# Stage 3 - Final Runtime
# ============================
FROM base AS final

WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "ByteLink.Api.dll"]
