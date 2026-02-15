# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Directory.Packages.props", "."]
COPY ["src/InvestLens.Auth/InvestLens.Auth.Api/InvestLens.Auth.Api.csproj", "src/InvestLens.Auth/InvestLens.Auth.Api/"]
COPY ["src/InvestLens.Shared/InvestLens.Shared.csproj", "src/InvestLens.Shared/"]
COPY ["src/InvestLens.Data/InvestLens.Data.Entities/InvestLens.Data.Entities.csproj", "src/InvestLens.Data/InvestLens.Data.Entities/"]
COPY ["src/InvestLens.Auth/InvestLens.Auth.DataContext/InvestLens.Auth.DataContext.csproj", "src/InvestLens.Auth/InvestLens.Auth.DataContext/"]
COPY ["src/InvestLens.Auth/InvestLens.Auth.Entities/InvestLens.Auth.Entities.csproj", "src/InvestLens.Auth/InvestLens.Auth.Entities/"]
RUN dotnet restore "./src/InvestLens.Auth/InvestLens.Auth.Api/InvestLens.Auth.Api.csproj"
COPY . .
WORKDIR "/src/src/InvestLens.Auth/InvestLens.Auth.Api"
RUN dotnet build "./InvestLens.Auth.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./InvestLens.Auth.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "InvestLens.Auth.Api.dll"]