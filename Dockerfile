# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Installation de Node.js (version 22) et npm
RUN apt-get update && apt-get install -y curl gnupg && \
    curl -fsSL https://deb.nodesource.com/setup_22.x | bash - && \
    apt-get install -y nodejs

# Installation du runtime .NET 6.0
RUN wget https://packages.microsoft.com/config/debian/$(lsb_release -rs)/packages-microsoft-prod.deb -O packages-microsoft-prod.deb && \
    dpkg -i packages-microsoft-prod.deb && \
    rm packages-microsoft-prod.deb && \
    apt-get update && \
    apt-get install -y dotnet-runtime-6.0

# Copie des fichiers projet
COPY *.csproj ./
RUN dotnet restore

# Copie du reste du code source
COPY . ./

# Ajout de la dépendance ElectronNET.API et installation de l'outil ElectronNET.CLI
RUN dotnet add package ElectronNET.API --version 23.6.2
RUN dotnet tool install --global ElectronNET.CLI --version 23.6.2

# Publication de l'application
RUN dotnet publish -c Release -r linux-x64 --self-contained false -o /app/publish

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0
ENV ASPNETCORE_URLS=http://*:${PORT:-8080}
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Facilys.dll"]
