FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5000

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["StabilityMatrix.ChatInterface/StabilityMatrix.ChatInterface.csproj", "StabilityMatrix.ChatInterface/"]
COPY ["StabilityMatrix.Core/StabilityMatrix.Core.csproj", "StabilityMatrix.Core/"]
RUN dotnet restore "StabilityMatrix.ChatInterface/StabilityMatrix.ChatInterface.csproj"
COPY . .
WORKDIR "/src/StabilityMatrix.ChatInterface"
RUN dotnet build "StabilityMatrix.ChatInterface.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "StabilityMatrix.ChatInterface.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "StabilityMatrix.ChatInterface.dll"]
