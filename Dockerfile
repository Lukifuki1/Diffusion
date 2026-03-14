FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5000

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/AuraFlow/AuraFlow.csproj", "src/AuraFlow/"]
COPY ["src/AuraFlow.Core/AuraFlow.Core.csproj", "src/AuraFlow.Core/"]
RUN dotnet restore "src/AuraFlow/AuraFlow.csproj"
COPY src/AuraFlow.Core/ ./src/AuraFlow.Core/
COPY src/AuraFlow/ ./src/AuraFlow/
WORKDIR "/src/src/AuraFlow"
RUN dotnet build "AuraFlow.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AuraFlow.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AuraFlow.dll"]
