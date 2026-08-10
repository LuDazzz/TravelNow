# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# ---------- base ----------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

# ---------- build ----------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy all project files first for better layer caching on restore
COPY ["TravelNow/TravelNow.csproj", "TravelNow/"]
COPY ["TravelNow.Application/TravelNow.Application.csproj", "TravelNow.Application/"]
COPY ["TravelNow.Domain/TravelNow.Domain.csproj", "TravelNow.Domain/"]
COPY ["TravelNow.Infrastructure/TravelNow.Infrastructure.csproj", "TravelNow.Infrastructure/"]
COPY ["TravelNow.Shared/TravelNow.Shared.csproj", "TravelNow.Shared/"]
RUN dotnet restore "./TravelNow/TravelNow.csproj"

COPY . .
WORKDIR "/src/TravelNow"
RUN dotnet build "./TravelNow.csproj" -c $BUILD_CONFIGURATION -o /app/build

# ---------- publish ----------
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./TravelNow.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# ---------- final ----------
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TravelNow.dll"]