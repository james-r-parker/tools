FROM alpine:3.18 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 5000

FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine3.18-amd64 AS build
RUN apk update && apk upgrade
RUN apk add --no-cache clang build-base zlib-dev libssl1.1
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["DotnetHelp.DevTools.csproj", "./"]
RUN dotnet restore "DotnetHelp.DevTools.csproj"
COPY . .
WORKDIR "/src/"
RUN dotnet build "DotnetHelp.DevTools.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "DotnetHelp.DevTools.csproj" -c $BUILD_CONFIGURATION -o /app/publish

FROM base AS final
RUN apk update && apk upgrade
RUN apk add --no-cache libstdc++
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["/app/DotnetHelp.DevTools"]
