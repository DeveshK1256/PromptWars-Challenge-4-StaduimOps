# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY src/StadiumOps.Domain/*.csproj src/StadiumOps.Domain/
COPY src/StadiumOps.Infrastructure/*.csproj src/StadiumOps.Infrastructure/
COPY src/StadiumOps.Api/*.csproj src/StadiumOps.Api/
RUN dotnet restore src/StadiumOps.Api/StadiumOps.Api.csproj

COPY src/ src/
RUN dotnet publish src/StadiumOps.Api/StadiumOps.Api.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS runtime
WORKDIR /app

RUN addgroup -S appgroup && adduser -S appuser -G appgroup

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080

USER appuser
HEALTHCHECK --interval=30s --timeout=5s --start-period=10s --retries=3 \
    CMD wget -qO- http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "StadiumOps.Api.dll"]
