# syntax=docker/dockerfile:1

# ---------- Build ----------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Restore first (cached layer as long as no project file changes)
COPY Directory.Build.props ./
COPY src/IdeaVault.Api/IdeaVault.Api.csproj src/IdeaVault.Api/
COPY src/IdeaVault.Core/IdeaVault.Core.csproj src/IdeaVault.Core/
COPY src/IdeaVault.Infra/IdeaVault.Infra.csproj src/IdeaVault.Infra/
COPY src/IdeaVault.Web/IdeaVault.Web.csproj src/IdeaVault.Web/
RUN dotnet restore src/IdeaVault.Api/IdeaVault.Api.csproj

COPY src/ src/
RUN dotnet publish src/IdeaVault.Api/IdeaVault.Api.csproj \
    --configuration Release --no-restore --output /app /p:UseAppHost=false

# ---------- Runtime ----------
# "Chiseled" = distroless Ubuntu: no shell or package manager, non-root by default, ~110 MB.
FROM mcr.microsoft.com/dotnet/aspnet:10.0-noble-chiseled AS runtime
WORKDIR /app
COPY --from=build /app .

ENV ASPNETCORE_ENVIRONMENT=Production \
    ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

# Render injects $PORT, which Program.cs honors; 8080 is the fallback.
ENTRYPOINT ["dotnet", "IdeaVault.Api.dll"]
