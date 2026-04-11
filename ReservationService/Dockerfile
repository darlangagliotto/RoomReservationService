FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj files and restore
COPY ["src/ReservationService.Api/ReservationService.Api.csproj", "src/ReservationService.Api/"]
COPY ["src/ReservationService.Infrastructure/ReservationService.Infrastructure.csproj", "src/ReservationService.Infrastructure/"]
COPY ["src/ReservationService.Domain/ReservationService.Domain.csproj", "src/ReservationService.Domain/"]
COPY ["src/ReservationService.Application/ReservationService.Application.csproj", "src/ReservationService.Application/"]

RUN dotnet restore "src/ReservationService.Api/ReservationService.Api.csproj"

# Copy everything and publish
COPY . .
WORKDIR /src/src/ReservationService.Api
RUN dotnet publish "ReservationService.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

RUN apt-get update \
	&& apt-get install -y curl \
	&& rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:5000
EXPOSE 5000

ENTRYPOINT ["dotnet", "ReservationService.Api.dll"]
