# Étape de build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# Copier uniquement le projet Blazor
WORKDIR /src
COPY . .
RUN dotnet restore
RUN dotnet publish -c Release -r linux-x64 --self-contained false

# Étape runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0
ENV ASPNETCORE_URLS=http://*:${PORT:-8080}
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Facilys.dll"]