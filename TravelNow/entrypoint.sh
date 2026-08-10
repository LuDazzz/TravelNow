#!/bin/sh
set -e

echo "Applying database migrations..."
dotnet ef database update --project /app/TravelNow.Infrastructure.csproj --startup-project /app/TravelNow.csproj --context TravelNowDbContext

echo "Starting application..."
exec dotnet TravelNow.dll