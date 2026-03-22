FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj files and restore
COPY ["src/RoomService.Api/RoomService.Api.csproj", "src/RoomService.Api/"]
COPY ["src/RoomService.Infrastructure/RoomService.Infrastructure.csproj", "src/RoomService.Infrastructure/"]
COPY ["src/RoomService.Domain/RoomService.Domain.csproj", "src/RoomService.Domain/"]
COPY ["src/RoomService.Application/RoomService.Application.csproj", "src/RoomService.Application/"]

RUN dotnet restore "src/RoomService.Api/RoomService.Api.csproj"

# Copy everything and publish
COPY . .
WORKDIR /src/src/RoomService.Api
RUN dotnet publish "RoomService.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:5000
EXPOSE 5000

ENTRYPOINT ["dotnet", "RoomService.Api.dll"]
