FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj files and restore
COPY ["src/UserService.Api/UserService.Api.csproj", "src/UserService.Api/"]
COPY ["src/UserService.Infrastructure/UserService.Infrastructure.csproj", "src/UserService.Infrastructure/"]
COPY ["src/UserService.Domain/UserService.Domain.csproj", "src/UserService.Domain/"]
COPY ["src/UserService.Application/UserService.Application.csproj", "src/UserService.Application/"]

RUN dotnet restore "src/UserService.Api/UserService.Api.csproj"

# Copy everything and publish
COPY . .
WORKDIR /src/src/UserService.Api
RUN dotnet publish "UserService.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Necessário para o healthcheck do Docker (curl http://localhost:5000/health)
RUN apt-get update \
	&& apt-get install -y curl \
	&& rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:5000
EXPOSE 5000

ENTRYPOINT ["dotnet", "UserService.Api.dll"]
