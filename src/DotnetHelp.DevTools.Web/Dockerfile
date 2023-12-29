FROM nginx:latest AS base
USER $APP_UID
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine3.18-amd64 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["DotnetHelp.DevTools.Web/DotnetHelp.DevTools.Web.csproj", "DotnetHelp.DevTools.Web/"]
RUN dotnet restore "DotnetHelp.DevTools.Web/DotnetHelp.DevTools.Web.csproj"
COPY . .
WORKDIR "/src/DotnetHelp.DevTools.Web"
RUN dotnet build "DotnetHelp.DevTools.Web.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "DotnetHelp.DevTools.Web.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish/wwwroot /usr/share/nginx/html
