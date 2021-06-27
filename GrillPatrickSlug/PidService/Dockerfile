#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["PidService/PidService.fsproj", "PidService/"]
COPY ["PidServiceModel/PidServiceModel.fsproj", "PidServiceModel/"]
RUN dotnet restore "PidService/PidService.fsproj"
COPY . .
WORKDIR "/src/PidService"
RUN dotnet build "PidService.fsproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "PidService.fsproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PidService.dll"]