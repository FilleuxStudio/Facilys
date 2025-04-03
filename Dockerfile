FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src/AppFacilys/Facilys/Facilys
COPY AppFacilys/Facilys/Facilys/*.csproj ./
RUN dotnet restore
COPY AppFacilys/Facilys/Facilys/ ./
RUN dotnet publish -c Release -r linux-x64 --self-contained false -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0
ENV ASPNETCORE_URLS=http://*:${PORT:-8080}
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Facilys.dll"]
