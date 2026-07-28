#!/bin/bash

# Exit immediately if a command exits with a non-zero status
set -e

# Default to Development if not already specified
export DOTNET_ENVIRONMENT="${DOTNET_ENVIRONMENT:-Development}"

MIGRATIONS_PROJ="./src/backend/Migrations/ByteLink.Migrations.csproj"

echo "🌍 Environment: $DOTNET_ENVIRONMENT"

echo "🧹 Cleaning previous build artifacts..."
dotnet clean "$MIGRATIONS_PROJ" -c Debug

echo "🔨 Rebuilding migration runner (forcing embedded resource refresh)..."
dotnet build "$MIGRATIONS_PROJ" -c Debug --no-incremental

echo "🚀 Running migrations..."
dotnet run \
    --project "$MIGRATIONS_PROJ" \
    --no-build \
    -c Debug \
    -- "$@"
