FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["src/QualityWaterAlert.WebApp/QualityWaterAlert.WebApp.csproj", "src/QualityWaterAlert.WebApp/"]
COPY ["src/QualityWaterAlert.Core/QualityWaterAlert.Core.csproj", "src/QualityWaterAlert.Core/"]
COPY ["src/QualityWaterAlert.Infrastructure/QualityWaterAlert.Infrastructure.csproj", "src/QualityWaterAlert.Infrastructure/"]

RUN dotnet restore "src/QualityWaterAlert.WebApp/QualityWaterAlert.WebApp.csproj"

COPY src/ src/

RUN dotnet publish "src/QualityWaterAlert.WebApp/QualityWaterAlert.WebApp.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

RUN addgroup --system --gid 1001 appgroup \
 && adduser --system --uid 1001 --ingroup appgroup appuser

COPY --from=build /app/publish .

USER appuser

EXPOSE 8080

ENTRYPOINT ["dotnet", "QualityWaterAlert.WebApp.dll"]
