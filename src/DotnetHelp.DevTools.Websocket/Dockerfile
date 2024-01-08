FROM public.ecr.aws/sam/build-provided.al2023:latest-arm64 AS build
RUN dnf install clang --assumeyes
RUN dnf install libicu --assumeyes
RUN dnf install zlib-devel --assumeyes
RUN dnf install wget --assumeyes

RUN wget https://dot.net/v1/dotnet-install.sh -O dotnet-install.sh
RUN chmod +x ./dotnet-install.sh
RUN ./dotnet-install.sh --version latest

ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["DotnetHelp.DevTools.Websocket/DotnetHelp.DevTools.Websocket.csproj", "DotnetHelp.DevTools.Websocket/"]
RUN $HOME/.dotnet/dotnet restore "DotnetHelp.DevTools.Websocket/DotnetHelp.DevTools.Websocket.csproj"
COPY . .
WORKDIR "/src/DotnetHelp.DevTools.Websocket"
RUN $HOME/.dotnet/dotnet build "DotnetHelp.DevTools.Websocket.csproj" -c $BUILD_CONFIGURATION -o /app/build -r linux-arm64

WORKDIR /src/DotnetHelp.DevTools.Websocket
ARG BUILD_CONFIGURATION=Release
RUN $HOME/.dotnet/dotnet publish -c $BUILD_CONFIGURATION -o /web-socket/publish -r linux-arm64

FROM scratch AS final
WORKDIR /web-socket
COPY --from=build /web-socket/publish .

