#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
RUN apt-get update && apt-get install curl -y
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["SensorService/SensorService.fsproj", "SensorService/"]
COPY ["SensorServiceModel/SensorServiceModel.fsproj", "SensorServiceModel/"]
RUN dotnet restore "SensorService/SensorService.fsproj"
COPY . .
WORKDIR "/src/SensorService"
RUN dotnet build "SensorService.fsproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SensorService.fsproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SensorService.dll"]