# build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app
COPY . .
RUN dotnet publish -c Release -o /out

# runtime
FROM nginx:alpine
COPY --from=build /out/wwwroot /usr/share/nginx/html
