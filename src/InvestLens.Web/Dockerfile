# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081
EXPOSE 8082

# Установите curl для health check
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Только после установки curl переключаемся на non-root пользователя
USER $APP_UID

# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Directory.Packages.props", "."]
COPY ["src/InvestLens.Web/InvestLens.Web.csproj", "src/InvestLens.Web/"]
COPY ["src/InvestLens.Shared/InvestLens.Shared.csproj", "src/InvestLens.Shared/"]
COPY ["src/InvestLens.Abstraction/InvestLens.Abstraction.csproj", "src/InvestLens.Abstraction/"]
COPY ["src/InvestLens.Data.Shared/InvestLens.Data.Shared.csproj", "src/InvestLens.Data.Shared/"]
COPY ["src/InvestLens.Data/InvestLens.Data.Entities/InvestLens.Data.Entities.csproj", "src/InvestLens.Data/InvestLens.Data.Entities/"]
RUN dotnet restore "./src/InvestLens.Web/InvestLens.Web.csproj"
COPY . .
WORKDIR "/src/src/InvestLens.Web"
RUN dotnet build "./InvestLens.Web.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./InvestLens.Web.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "InvestLens.Web.dll"]