FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["CreateGlibb.Api/CreateGlibb.Api.csproj", "CreateGlibb.Api/"]
COPY ["Contracts/Contracts.csproj", "Contracts/"]
COPY ["Library.Redis/Library.Redis.csproj", "Library.Redis/"]
RUN dotnet restore "CreateGlibb.Api/CreateGlibb.Api.csproj"
COPY . .
WORKDIR "/src/CreateGlibb.Api"
RUN dotnet build "CreateGlibb.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CreateGlibb.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CreateGlibb.Api.dll"]