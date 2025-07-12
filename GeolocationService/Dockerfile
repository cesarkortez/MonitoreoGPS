FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS base
ENV ASPNETCORE_URLS=http://+:80
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["MonitoreoGPS.Shared/MonitoreoGPS.Shared.csproj", "MonitoreoGPS.Shared/"]
COPY ["GeolocationService/GeolocationService.csproj", "./"]
RUN dotnet restore "GeolocationService.csproj"

COPY . .
WORKDIR "/src/GeolocationService"
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "GeolocationService.dll"]
