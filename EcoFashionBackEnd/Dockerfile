FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["EcoFashionBackEnd/EcoFashionBackEnd.csproj", "EcoFashionBackEnd/"]
RUN dotnet restore "EcoFashionBackEnd/EcoFashionBackEnd.csproj"

COPY . .
WORKDIR "/src/EcoFashionBackEnd"
RUN dotnet build "EcoFashionBackEnd.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "EcoFashionBackEnd.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EcoFashionBackEnd.dll"]
