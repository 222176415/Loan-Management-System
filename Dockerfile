# 1. Base Image for running the app (.NET 9)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

# 2. SDK Image for building the code (.NET 9)
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj files and restore distinct layers for caching
COPY ["LoanManagementSystem.Api/LoanManagementSystem.Api.csproj", "LoanManagementSystem.Api/"]
COPY ["LoanManagementSystem.Infrastructure/LoanManagementSystem.Infrastructure.csproj", "LoanManagementSystem.Infrastructure/"]
COPY ["LoanManagementSystem.Application/LoanManagementSystem.Application.csproj", "LoanManagementSystem.Application/"]
COPY ["LoanManagementSystem.Domain/LoanManagementSystem.Domain.csproj", "LoanManagementSystem.Domain/"]

RUN dotnet restore "LoanManagementSystem.Api/LoanManagementSystem.Api.csproj"

# Copy full source code and build
COPY . .
WORKDIR "/src/LoanManagementSystem.Api"
RUN dotnet build "LoanManagementSystem.Api.csproj" -c Release -o /app/build

# 3. Publish stage
FROM build AS publish
RUN dotnet publish "LoanManagementSystem.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# 4. Final runtime stage
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "LoanManagementSystem.Api.dll"]