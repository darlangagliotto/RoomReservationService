FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj files and restore
COPY ["src/Gateway.Api/Gateway.Api.csproj", "src/Gateway.Api/"]

RUN dotnet restore "src/Gateway.Api/Gateway.Api.csproj"

# Copy everything and publish
COPY . .
WORKDIR /src/src/Gateway.Api
RUN dotnet publish "Gateway.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Necessário para o healthcheck do Docker (curl http://localhost:5000/health)
RUN apt-get update \
	&& apt-get install -y curl \
	&& rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:5000
EXPOSE 5000

ENTRYPOINT ["dotnet", "Gateway.Api.dll"]