FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["CreateGlibb.MessageService/CreateGlibb.MessageService.csproj", "CreateGlibb.MessageService/"]
COPY ["Contracts/Contracts.csproj", "Contracts/"]
COPY ["Library.Redis/Library.Redis.csproj", "Library.Redis/"]
RUN dotnet restore "CreateGlibb.MessageService/CreateGlibb.MessageService.csproj"
COPY . .
WORKDIR "/src/CreateGlibb.MessageService"
RUN dotnet build "CreateGlibb.MessageService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CreateGlibb.MessageService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CreateGlibb.MessageService.dll"]