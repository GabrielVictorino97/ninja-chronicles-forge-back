FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY src/NarutoPlayers.Domain/*.csproj NarutoPlayers.Domain/
COPY src/NarutoPlayers.Contracts/*.csproj NarutoPlayers.Contracts/
COPY src/NarutoPlayers.Application/*.csproj NarutoPlayers.Application/
COPY src/NarutoPlayers.Infrastructure/*.csproj NarutoPlayers.Infrastructure/
COPY src/NarutoPlayers.Api/*.csproj NarutoPlayers.Api/
RUN dotnet restore NarutoPlayers.Api/NarutoPlayers.Api.csproj

COPY src/ .
RUN dotnet publish NarutoPlayers.Api/NarutoPlayers.Api.csproj -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "NarutoPlayers.Api.dll"]
