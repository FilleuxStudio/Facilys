# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

RUN apt-get update && apt-get install -y \
    curl \
    gnupg \
    lsb-release \
    libx11-dev \
    libxext-dev \
    libxss-dev \
    libgtk-3-dev \
    libgconf-2-4 \
    libc6-dev \
    libgdiplus

# Installation de Node.js (version 22) et npm
RUN apt-get update && apt-get install -y curl gnupg && \
    curl -fsSL https://deb.nodesource.com/setup_22.x | bash - && \
    apt-get install -y nodejs

# Copie des fichiers projet (.csproj)
COPY *.csproj ./
RUN dotnet restore

# Copie du reste du code source
COPY . ./

# Ajout de la dépendance ElectronNET.API et installation de l'outil global ElectronNET.CLI
RUN dotnet add package ElectronNET.API --version 23.6.2

# Publication de l'application
RUN mkdir -p /app/publish
RUN dotnet publish -c Release -o /app/publish -p:IsWebBuild=true -p:PublishReadyToRun=true
RUN ls -all
RUN ls /app/publish

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0

# Dépendances runtime pour Electron
RUN apt-get update && apt-get install -y \
    libx11-6 \
    libxext6 \
    libxss1 \
    libgtk-3-0 \
    libgconf-2-4 \
    libc6 \
    libgdiplus

ENV ASPNETCORE_URLS=http://*:${PORT:-8080}
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Facilys.dll"]
