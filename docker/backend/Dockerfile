# syntax=docker/dockerfile:1-labs

# ============================
# Stage 1 - Base Runtime
# ============================
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base

WORKDIR /app
EXPOSE 8080

USER root
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*
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

# Copies ALL .csproj files inside 'src' and preserves their folder structure
COPY --parents src/**/*.csproj ./

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

ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "ByteLink.Api.dll"]
