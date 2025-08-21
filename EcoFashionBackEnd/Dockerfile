FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

ARG BUILD_CONFIGURATION=Release

RUN apt-get update \
    && apt-get install -y --no-install-recommends \
    clang zlib1g-dev

WORKDIR /src

COPY ["EcoFashionBackEnd/EcoFashionBackEnd.csproj","EcoFashionBackEnd/"]

RUN dotnet restore "EcoFashionBackEnd/EcoFashionBackEnd.csproj"

COPY . .
WORKDIR "/src/EcoFashionBackEnd"
RUN dotnet build "EcoFashionBackEnd.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "EcoFashionBackEnd.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=true

FROM mcr.microsoft.com/dotnet/runtime-deps:8.0 AS final
WORKDIR /app
EXPOSE 8080
COPY --from=publish /app/publish .
ENTRYPOINT ["./EcoFashionBackEnd"]