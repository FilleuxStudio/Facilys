# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
# Copy only the project file(s)
COPY *.csproj ./
RUN dotnet restore
RUN ls -all
# Copy the remaining source code
COPY . ./
RUN ls -all
RUN dotnet publish -c Release -r linux-x64 --self-contained false -o /app/publish

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0
ENV ASPNETCORE_URLS=http://*:${PORT:-8080}
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Facilys.dll"]
