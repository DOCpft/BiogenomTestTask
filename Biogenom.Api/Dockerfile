# Базовый образ для запуска
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV DOTNET_RUNNING_IN_CONTAINER=true

# Этап сборки
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Biogenom.Api/Biogenom.Api.csproj", "Biogenom.Api/"]
RUN dotnet restore "Biogenom.Api/Biogenom.Api.csproj"
COPY . .
WORKDIR "/src/Biogenom.Api"
ARG CONFIGURATION=Release
RUN dotnet publish "Biogenom.Api.csproj" -c $CONFIGURATION -o /app/publish /p:UseAppHost=false

# Финальный образ
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Biogenom.Api.dll"]