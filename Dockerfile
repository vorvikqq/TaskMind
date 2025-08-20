# Use .NET SDK for building
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files
COPY *.sln .
COPY src/TaskMind.Web/*.csproj src/TaskMind.Web/
COPY src/TaskMind.Domain/*.csproj src/TaskMind.Domain/
COPY src/TaskMind.Application/*.csproj src/TaskMind.Application/
COPY src/TaskMind.Infrastructure/*.csproj src/TaskMind.Infrastructure/
COPY tests/TaskMind.Web.Tests/*.csproj tests/TaskMind.Web.Tests/
COPY tests/TaskMind.Application.Tests/*.csproj tests/TaskMind.Application.Tests/
COPY tests/TaskMind.Infrastructure.Tests/*.csproj tests/TaskMind.Infrastructure.Tests/

# Restore dependencies
RUN dotnet restore

# Copy all source code
COPY . .

# Build the web project
WORKDIR /src/src/TaskMind.Web
RUN dotnet build -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

# Create runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "TaskMind.Web.dll"]