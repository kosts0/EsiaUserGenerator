# ---------- build ----------
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# копируем ТОЛЬКО csproj
COPY EsiaUserBff/*.csproj ./EsiaUserBff/
RUN dotnet restore EsiaUserBff/EsiaUserBff.csproj

# копируем остальной код
COPY EsiaUserBff/. ./EsiaUserBff/
WORKDIR /src/EsiaUserBff

RUN dotnet publish -c Release -o /app/publish

# ---------- runtime ----------
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app

ENV ASPNETCORE_URLS=http://0.0.0.0:8080

COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "EsiaUserBff.dll"]
