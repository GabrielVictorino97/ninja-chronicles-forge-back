FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY src/KageNoTessen.Domain/*.csproj KageNoTessen.Domain/
COPY src/KageNoTessen.Contracts/*.csproj KageNoTessen.Contracts/
COPY src/KageNoTessen.Application/*.csproj KageNoTessen.Application/
COPY src/KageNoTessen.Infrastructure/*.csproj KageNoTessen.Infrastructure/
COPY src/KageNoTessen.Api/*.csproj KageNoTessen.Api/
RUN dotnet restore KageNoTessen.Api/KageNoTessen.Api.csproj

COPY src/ .
RUN dotnet publish KageNoTessen.Api/KageNoTessen.Api.csproj -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "KageNoTessen.Api.dll"]
