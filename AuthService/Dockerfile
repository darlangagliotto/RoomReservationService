FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj files and restore
COPY ["src/AuthService.Api/AuthService.Api.csproj", "src/AuthService.Api/"]
COPY ["src/AuthService.Infrastructure/AuthService.Infrastructure.csproj", "src/AuthService.Infrastructure/"]
COPY ["src/AuthService.Domain/AuthService.Domain.csproj", "src/AuthService.Domain/"]
COPY ["src/AuthService.Application/AuthService.Application.csproj", "src/AuthService.Application/"]

RUN dotnet restore "src/AuthService.Api/AuthService.Api.csproj"

# Copy everything and publish
COPY . .
WORKDIR /src/src/AuthService.Api
RUN dotnet publish "AuthService.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:5000
EXPOSE 5000

ENTRYPOINT ["dotnet", "AuthService.Api.dll"]
