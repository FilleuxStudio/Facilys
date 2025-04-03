# Étape de build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# Copier uniquement le projet Blazor
WORKDIR /src
COPY . .
RUN dotnet restore "Facilys.csproj"
RUN dotnet publish "Facilys.csproj" -c Release -o /app/publish --no-self-contained

# Étape runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0
ENV ASPNETCORE_URLS=http://*:${PORT:-8080}
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Facilys.dll"]