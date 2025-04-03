# Étape de build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# Définition du répertoire de travail
WORKDIR /src

# Copier uniquement le fichier .csproj pour optimiser la mise en cache
COPY AppFacilys/Facilys/Facilys/*.csproj ./AppFacilys/Facilys/Facilys/
RUN dotnet restore ./AppFacilys/Facilys/Facilys/Facilys.csproj

# Copier le reste du projet
COPY AppFacilys/Facilys/Facilys/ ./AppFacilys/Facilys/Facilys/

# Construire et publier l'application pour Linux
WORKDIR /src/AppFacilys/Facilys/Facilys
RUN dotnet publish "Facilys.csproj" -c Release -o /app/publish --no-self-contained

# Étape finale (runtime)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

ENV ASPNETCORE_URLS=http://*:${PORT:-8080}
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Facilys.dll"]