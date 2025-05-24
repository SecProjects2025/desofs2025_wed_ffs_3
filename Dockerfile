FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

WORKDIR /src

COPY AMAP.sln .
COPY AMAP/AMAP.Business/*.csproj AMAP/AMAP.Business/
COPY AMAP/AMAP.Data/*.csproj AMAP/AMAP.Data/
COPY AMAP/AMAP.Models/*.csproj AMAP/AMAP.Models/
COPY AMAP/AMAP.UnitTests/*.csproj AMAP/AMAP.UnitTests/
COPY AMAP/AMAP_API/*.csproj AMAP/AMAP_API/

RUN dotnet restore AMAP.sln

COPY AMAP/ AMAP/

WORKDIR /src/AMAP/AMAP_API
RUN dotnet publish "AMAP.Api.csproj" -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final

WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "AMAP_API.dll"] 