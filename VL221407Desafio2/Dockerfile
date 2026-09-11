FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Restore separado para reutilizar la caché cuando sólo cambia el código.
COPY Directory.Build.props ./
COPY API/VL221407Desafio2.API.csproj API/
COPY BL/VL221407Desafio2.BL.csproj BL/
COPY DAL/VL221407Desafio2.DAL.csproj DAL/
COPY DTOs/VL221407Desafio2.DTOs.csproj DTOs/
COPY Entities/VL221407Desafio2.Entities.csproj Entities/
COPY Common/VL221407Desafio2.Common.csproj Common/
RUN dotnet restore API/VL221407Desafio2.API.csproj
COPY . .
RUN dotnet publish API/VL221407Desafio2.API.csproj \
    -c Release --no-restore -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*
COPY --from=build /app/publish .
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
USER $APP_UID
HEALTHCHECK --interval=30s --timeout=5s --start-period=20s --retries=3 \
    CMD curl --fail --silent http://127.0.0.1:8080/health/live || exit 1
ENTRYPOINT ["dotnet", "VL221407Desafio2.API.dll"]
