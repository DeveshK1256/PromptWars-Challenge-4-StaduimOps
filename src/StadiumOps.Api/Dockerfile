FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY StadiumOps.slnx ./
COPY Directory.Build.props ./
COPY src ./src
RUN dotnet restore StadiumOps.slnx
RUN dotnet publish src/StadiumOps.Api/StadiumOps.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://0.0.0.0:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "StadiumOps.Api.dll"]
