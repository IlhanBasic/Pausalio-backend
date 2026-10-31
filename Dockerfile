
# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["Pausalio.API/Pausalio.API.csproj", "Pausalio.API/"]
COPY ["Pausalio.Application/Pausalio.Application.csproj", "Pausalio.Application/"]
COPY ["Pausalio.Domain/Pausalio.Domain.csproj", "Pausalio.Domain/"]
COPY ["Pausalio.Infrastructure/Pausalio.Infrastructure.csproj", "Pausalio.Infrastructure/"]
COPY ["Pausalio.Shared/Pausalio.Shared.csproj", "Pausalio.Shared/"]

RUN dotnet restore "Pausalio.API/Pausalio.API.csproj"

COPY . .
RUN dotnet publish "Pausalio.API/Pausalio.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final

USER root

RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /app

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Pausalio.API.dll"]
