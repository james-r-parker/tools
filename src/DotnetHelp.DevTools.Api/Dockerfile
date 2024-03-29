﻿FROM public.ecr.aws/sam/build-provided.al2023:latest-x86_64 AS build

RUN dnf install clang libicu zlib-devel wget --assumeyes
RUN rpm --import https://packages.microsoft.com/keys/microsoft.asc
RUN wget -O /etc/yum.repos.d/microsoft-prod.repo https://packages.microsoft.com/config/fedora/37/prod.repo
RUN dnf install dotnet-sdk-8.0 --assumeyes
ENV DOTNET_CLI_TELEMETRY_OPTOUT=1
ENV DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1
ENV DOTNET_NOLOGO=1

ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["DotnetHelp.DevTools.Api/DotnetHelp.DevTools.Api.csproj", "DotnetHelp.DevTools.Api/"]
COPY ["DotnetHelp.DevTools.Shared/DotnetHelp.DevTools.Shared.csproj", "DotnetHelp.DevTools.Shared/"]
COPY ["DotnetHelp.DevTools.Cache/DotnetHelp.DevTools.Cache.csproj", "DotnetHelp.DevTools.Cache/"]
COPY ["DotnetHelp.DevTools.WebsocketClient/DotnetHelp.DevTools.WebsocketClient.csproj", "DotnetHelp.DevTools.WebsocketClient/"]
RUN dotnet restore "DotnetHelp.DevTools.Api/DotnetHelp.DevTools.Api.csproj"
COPY . .
WORKDIR "/src/DotnetHelp.DevTools.Api"
RUN dotnet build "DotnetHelp.DevTools.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build 

WORKDIR /src/DotnetHelp.DevTools.Api
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish -c $BUILD_CONFIGURATION -o /app/publish

FROM scratch AS final
WORKDIR /app
COPY --from=build /app/publish .

