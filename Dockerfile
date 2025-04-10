# Étape 1: Construction
FROM debian:latest AS build

# Configuration de l'environnement
ENV DEBIAN_FRONTEND=noninteractive \
    DOTNET_CLI_TELEMETRY_OPTOUT=1 \
    NODE_VERSION=22.x \
    ASPNETCORE_ENVIRONMENT=Production \
    ASPNETCORE_URLS=http://*:8080 \
    TESSDATA_PREFIX=/usr/share/tesseract-ocr/4.00/tessdata

# Installation des dépendances système et .NET
RUN apt-get update && \
    apt-get install -y --no-install-recommends \
    ca-certificates \
    curl \
    gnupg2 \
    libc6-dev \
    libgdiplus \
    libgtk-3-dev \
    libx11-dev \
    libxext-dev \
    libxss-dev \
    libleptonica-dev \
    libtesseract-dev \
    libopencv-dev \
    tesseract-ocr-fra \
    && rm -rf /var/lib/apt/lists/*

# Installation de .NET SDK 8.0.408
RUN curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin \
    -Channel 8.0 \
    -Version 8.0.408 \
    -InstallDir /usr/share/dotnet \
    && ln -s /usr/share/dotnet/dotnet /usr/bin/dotnet

# Installation de Node.js 22
RUN curl -fsSL https://deb.nodesource.com/setup_22.x | bash - && \
    apt-get install -y nodejs

WORKDIR /src
COPY AppFacilys/Facilys/. .

WORKDIR /src/Facilys
RUN mkdir -p app/publish
# Restauration des dépendances .NET
RUN dotnet restore
RUN dotnet restore --runtime linux-x64
# Installation des dépendances ElectronNET
RUN dotnet add package ElectronNET.API --version 23.6.2

# Construction et publication
RUN dotnet clean && dotnet build && dotnet publish -c Release --runtime linux-x64 --self-contained false

# Étape 2: Exécution
FROM debian:latest

# Configuration des dépôts Microsoft
RUN apt-get update && \
    apt-get install -y --no-install-recommends \
    ca-certificates \
    wget \
    gnupg \
    && rm -rf /var/lib/apt/lists/*

# Mise à jour des certificats SSL
RUN update-ca-certificates

# Ajout du dépôt Microsoft (avec option --no-check-certificate en dernier recours)
RUN wget --no-check-certificate https://packages.microsoft.com/config/debian/12/packages-microsoft-prod.deb -O packages-microsoft-prod.deb && \
    dpkg -i packages-microsoft-prod.deb && \
    rm packages-microsoft-prod.deb

# Installation des dépendances runtime
RUN apt-get update && \
    apt-get install -y --no-install-recommends \
    aspnetcore-runtime-8.0 \
    dotnet-runtime-8.0 \
    libgdiplus \
    libtesseract-dev \
    tesseract-ocr-fra \
    && rm -rf /var/lib/apt/lists/*

ENV ASPNETCORE_URLS=http://*:8080 \
    TESSDATA_PREFIX=/usr/share/tesseract-ocr/4.00/tessdata

WORKDIR /app
COPY --from=build app/publish .

ENTRYPOINT ["dotnet", "Facilys.dll"]