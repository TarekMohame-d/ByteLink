# ============================
# Stage 1 - Base Runtime
# ============================
FROM mcr.microsoft.com/dotnet/runtime:10.0 AS base

WORKDIR /app

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

# Copy migrator csproj
COPY src/backend/Migrations/ByteLink.Migrations.csproj src/backend/Migrations/

# Cache NuGet packages across builds
RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages \
    dotnet restore src/backend/Migrations/ByteLink.Migrations.csproj

# Copy remaining source files
COPY . .

# Publish compiled binaries
RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages \
    dotnet publish src/backend/Migrations/ByteLink.Migrations.csproj \
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

ENTRYPOINT ["dotnet", "ByteLink.Migrations.dll"]
