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
COPY ["DotnetHelp.DevTools.Api.PreTraffic/DotnetHelp.DevTools.Api.PreTraffic.csproj", "DotnetHelp.DevTools.Api.PreTraffic/"]
RUN $HOME/.dotnet/dotnet restore "DotnetHelp.DevTools.Api.PreTraffic/DotnetHelp.DevTools.Api.PreTraffic.csproj"
COPY . .
WORKDIR "/src/DotnetHelp.DevTools.Api.PreTraffic"
RUN $HOME/.dotnet/dotnet build "DotnetHelp.DevTools.Api.PreTraffic.csproj" -c $BUILD_CONFIGURATION -o /app/build -r linux-arm64

WORKDIR /src/DotnetHelp.DevTools.Api.PreTraffic
ARG BUILD_CONFIGURATION=Release
RUN $HOME/.dotnet/dotnet publish -c $BUILD_CONFIGURATION -o /pre-traffic/publish -r linux-arm64

FROM scratch AS final
WORKDIR /pre-traffic
COPY --from=build /pre-traffic/publish .

