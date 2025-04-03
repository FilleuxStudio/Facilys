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
    libgdiplus \
    libtesseract-dev \
    libleptonica-dev \
    libopencv-dev \
    tesseract-ocr-fra \
    libmysqlclient-dev \
    libgdiplus

# Installation de Node.js (version 22) et npm
RUN apt-get update && apt-get install -y curl gnupg && \
    curl -fsSL https://deb.nodesource.com/setup_22.x | bash - && \
    apt-get install -y nodejs

# Configuration spécifique pour OpenCV sur Linux
ENV OPENCV_DLL_DIR=/usr/lib/x86_64-linux-gnu
ENV LD_LIBRARY_PATH=$LD_LIBRARY_PATH:$OPENCV_DLL_DIR

# Copie des fichiers projet (.csproj)
COPY *.csproj ./
RUN dotnet restore

# Copie du reste du code source
COPY . ./

# Ajout de la dépendance ElectronNET.API et installation de l'outil global ElectronNET.CLI
RUN dotnet add package ElectronNET.API --version 23.6.2

# Publication de l'application
RUN mkdir -p /app/publish
RUN dotnet publish -c Release -o /app/publish --runtime linux-x64 --self-contained false
RUN ls -all
RUN ls /app/publish

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0

# Dépendances runtime minimales
RUN apt-get update && \
    apt-get install -y \
    libgdiplus \
    libtesseract-dev \
    tesseract-ocr-fra \
    && rm -rf /var/lib/apt/lists/*

ENV ASPNETCORE_URLS=http://*:${PORT:-8080}
ENV TESSDATA_PREFIX=/usr/share/tesseract-ocr/4.00/tessdata
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Facilys.dll"]
