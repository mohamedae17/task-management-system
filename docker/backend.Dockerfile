# Multi-stage build for the ASP.NET Core 8 API.
# Build context: repository root.

# ---- build ----
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Restore (copy only the project files first for better layer caching).
COPY Directory.Build.props Directory.Packages.props TaskManagementSystem.sln ./
COPY src/TaskManagement.Domain/TaskManagement.Domain.csproj          src/TaskManagement.Domain/
COPY src/TaskManagement.Application/TaskManagement.Application.csproj src/TaskManagement.Application/
COPY src/TaskManagement.Infrastructure/TaskManagement.Infrastructure.csproj src/TaskManagement.Infrastructure/
COPY src/TaskManagement.Api/TaskManagement.Api.csproj                 src/TaskManagement.Api/
RUN dotnet restore TaskManagementSystem.sln

# Copy the rest and publish.
COPY src/ src/
RUN dotnet publish src/TaskManagement.Api/TaskManagement.Api.csproj \
    -c Release -o /app/publish \
    --no-restore /p:UseAppHost=false

# ---- runtime ----
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# Storage volumes mounted by docker-compose
RUN mkdir -p /app/uploads /app/mail-pickup /app/logs

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 8080

ENTRYPOINT ["dotnet", "TaskManagement.Api.dll"]
