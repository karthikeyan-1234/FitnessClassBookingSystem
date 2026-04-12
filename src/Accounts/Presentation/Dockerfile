# Base stage for running the app
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy project files for restoration (Optimizes Docker layer caching)
COPY ["Accounts/Presentation/Presentation.csproj", "Accounts/Presentation/"]
COPY ["Accounts/Application/Application.csproj", "Accounts/Application/"]
COPY ["Accounts/Domain/Domain.csproj", "Accounts/Domain/"]
COPY ["Accounts/Infrastructure/Infrastructure.csproj", "Accounts/Infrastructure/"]
# If you have shared projects, add them here:
# COPY ["Shared/Shared.csproj", "Shared/"]

# Restore dependencies
RUN dotnet restore "Accounts/Presentation/Presentation.csproj"

# Copy the entire source code
COPY . .

# Build the Presentation project
WORKDIR "/src/Accounts/Presentation"
RUN dotnet build "Presentation.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Presentation.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final stage
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Presentation.dll"]