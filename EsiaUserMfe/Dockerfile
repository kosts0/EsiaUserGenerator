# ---------- build ----------
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY . .
RUN dotnet publish -c Release -o /app/publish

# ---------- runtime ----------
FROM nginx:alpine
WORKDIR /usr/share/nginx/html

# чистим дефолт
RUN rm -rf ./*

# копируем билд
COPY --from=build /app/publish/wwwroot .

# копируем nginx конфиг
COPY nginx.conf /etc/nginx/nginx.conf

EXPOSE 80
