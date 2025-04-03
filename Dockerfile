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

# Installation des outils nécessaires pour récupérer le runtime .NET 6.0
# (lsb-release est installé pour que la substitution de commande fonctionne)
RUN apt-get update && apt-get install -y wget gnupg2 lsb-release

# Installation du runtime .NET 6.0
RUN wget https://packages.microsoft.com/config/debian/$(lsb_release -rs)/packages-microsoft-prod.deb -O packages-microsoft-prod.deb && \
    dpkg -i packages-microsoft-prod.deb && \
    rm packages-microsoft-prod.deb && \
    apt-get update && \
    apt-get install -y dotnet-runtime-6.0

# Copie des fichiers projet (.csproj)
COPY *.csproj ./
RUN dotnet restore
RUN ls -all

# Copie du reste du code source
COPY . ./
RUN ls -all

# Ajout de la dépendance ElectronNET.API et installation de l'outil global ElectronNET.CLI
RUN dotnet add package ElectronNET.API --version 23.6.2
RUN dotnet tool install --global ElectronNET.CLI --version 23.6.2

# Publication de l'application
RUN dotnet publish -c Release -r linux-x64 --self-contained false -o /app/publish --verbosity diag

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

# Installation du runtime .NET 6
RUN apt-get install -y wget && \
    wget https://packages.microsoft.com/config/debian/$(awk -F'=' '/VERSION_CODENAME/{print $2}' /etc/os-release)/packages-microsoft-prod.deb -O packages-microsoft-prod.deb && \
    dpkg -i packages-microsoft-prod.deb && \
    rm packages-microsoft-prod.deb && \
    apt-get update && \
    apt-get install -y dotnet-runtime-6.0

ENV ASPNETCORE_URLS=http://*:${PORT:-8080}
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Facilys.dll"]
