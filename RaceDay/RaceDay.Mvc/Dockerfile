FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

#  Notice the "RaceDay/" prefix on these paths!
COPY ["RaceDay/RaceDay.Mvc/RaceDay.Mvc.csproj", "RaceDay/RaceDay.Mvc/"]
COPY ["RaceDay/RaceDay.Contracts/RaceDay.Contracts.csproj", "RaceDay/RaceDay.Contracts/"]

RUN dotnet restore "RaceDay/RaceDay.Mvc/RaceDay.Mvc.csproj"

# Copy everything else
COPY . .
WORKDIR "/src/RaceDay/RaceDay.Mvc"
RUN dotnet build "RaceDay.Mvc.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "RaceDay.Mvc.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RaceDay.Mvc.dll"]