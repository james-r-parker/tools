FROM public.ecr.aws/sam/build-provided.al2023:latest-x86_64 AS build
RUN dnf install clang libicu zlib-devel wget --assumeyes

RUN rpm --import https://packages.microsoft.com/keys/microsoft.asc
RUN wget -O /etc/yum.repos.d/microsoft-prod.repo https://packages.microsoft.com/config/fedora/37/prod.repo
RUN dnf install -y dotnet-sdk-8.0

ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["DotnetHelp.DevTools.Websocket/DotnetHelp.DevTools.Websocket.csproj", "DotnetHelp.DevTools.Websocket/"]
RUN dotnet restore "DotnetHelp.DevTools.Websocket/DotnetHelp.DevTools.Websocket.csproj"
COPY . .
WORKDIR "/src/DotnetHelp.DevTools.Websocket"
RUN dotnet build "DotnetHelp.DevTools.Websocket.csproj" -c $BUILD_CONFIGURATION -o /app/build

WORKDIR /src/DotnetHelp.DevTools.Websocket
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish -c $BUILD_CONFIGURATION -o /web-socket/publish

FROM scratch AS final
WORKDIR /web-socket
COPY --from=build /web-socket/publish .

