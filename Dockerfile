FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

WORKDIR /src

COPY AMAP/AMAP.sln .
COPY AMAP/AMAP.Business/AMAP.Business.csproj AMAP.Business/
COPY AMAP/AMAP.Data/AMAP.DataAccess.csproj AMAP.Data/
COPY AMAP/AMAP.Models/AMAP.Entities.csproj AMAP.Models/
COPY AMAP/AMAP.UnitTests/AMAP.UnitTests.csproj AMAP.UnitTests/
COPY AMAP/AMAP_API/AMAP.Api.csproj AMAP_API/

RUN dotnet restore AMAP.sln

COPY AMAP/AMAP.Business/ AMAP.Business/
COPY AMAP/AMAP.Data/ AMAP.Data/
COPY AMAP/AMAP.Models/ AMAP.Models/
COPY AMAP/AMAP.UnitTests/ AMAP.UnitTests/
COPY AMAP/AMAP_API/ AMAP_API/

WORKDIR /src/AMAP_API
RUN dotnet publish "AMAP.Api.csproj" -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final

WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "AMAP_API.dll"] 