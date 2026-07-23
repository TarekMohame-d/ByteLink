#!/bin/bash

# Exit immediately if a command exits with a non-zero status
set -e

MIGRATIONS_PROJ="./src/backend/Migrations/ByteLink.Migrations.csproj"

echo "🧹 Cleaning previous build artifacts..."
dotnet clean $MIGRATIONS_PROJ -c Debug

echo "🔨 Rebuilding migration runner (forcing embedded resource refresh)..."
dotnet build $MIGRATIONS_PROJ -c Debug --no-incremental

echo "🚀 Running migrations..."
dotnet run --project $MIGRATIONS_PROJ --no-build -c Debug -- "$@"
