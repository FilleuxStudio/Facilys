# Étape 1: Construction
FROM debian:latest AS build

ENV DEBIAN_FRONTEND=noninteractive \
    DOTNET_CLI_TELEMETRY_OPTOUT=1 \
    NODE_VERSION=22.x \
    ASPNETCORE_ENVIRONMENT=Production \
    ASPNETCORE_URLS=http://*:8080 \
    TESSDATA_PREFIX=/usr/share/tesseract-ocr/4.00/tessdata

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
    tesseract-ocr-fra

RUN apt-get update && apt-get install -y openssh-client 

RUN curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin \
    -Channel 8.0 \
    -Version 8.0.408 \
    -InstallDir /usr/share/dotnet \
    && ln -s /usr/share/dotnet/dotnet /usr/bin/dotnet

RUN curl -fsSL https://deb.nodesource.com/setup_22.x | bash - && \
    apt-get install -y nodejs

RUN rm -rf /var/lib/apt/lists/*

WORKDIR /src
COPY AppFacilys/Facilys/. .
WORKDIR /src/Facilys

# Restauration des dépendances
RUN dotnet restore
RUN dotnet restore --runtime linux-x64
RUN dotnet add package ElectronNET.API --version 23.6.2

# Build & publish dans /app/publish
RUN dotnet publish -c Release --self-contained true --no-restore --runtime linux-x64  -o /app/publish

# Étape 2: Exécution
FROM debian:latest

RUN apt-get update && \
    apt-get install -y --no-install-recommends \
    ca-certificates \
    wget \
    gnupg 

RUN update-ca-certificates

RUN wget --no-check-certificate https://packages.microsoft.com/config/debian/12/packages-microsoft-prod.deb -O packages-microsoft-prod.deb && \
    dpkg -i packages-microsoft-prod.deb && \
    rm packages-microsoft-prod.deb

RUN apt-get update && \
    apt-get install -y --no-install-recommends \
    aspnetcore-runtime-8.0 \
    dotnet-runtime-8.0 \
    libgdiplus \
    libtesseract-dev \
    tesseract-ocr-fra \
    && rm -rf /var/lib/apt/lists/*

ENV PORT=8080 \
    TESSDATA_PREFIX=/usr/share/tesseract-ocr/4.00/tessdata

WORKDIR /app

EXPOSE $PORT

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Facilys.dll"]
