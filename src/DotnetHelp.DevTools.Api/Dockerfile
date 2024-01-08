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
COPY ["DotnetHelp.DevTools.Api/DotnetHelp.DevTools.Api.csproj", "DotnetHelp.DevTools.Api/"]
COPY ["DotnetHelp.DevTools.Shared/DotnetHelp.DevTools.Shared.csproj", "DotnetHelp.DevTools.Shared/"]
RUN $HOME/.dotnet/dotnet restore "DotnetHelp.DevTools.Api/DotnetHelp.DevTools.Api.csproj"
COPY . .
WORKDIR "/src/DotnetHelp.DevTools.Api"
RUN $HOME/.dotnet/dotnet build "DotnetHelp.DevTools.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build -r linux-arm64

WORKDIR /src/DotnetHelp.DevTools.Api
ARG BUILD_CONFIGURATION=Release
RUN $HOME/.dotnet/dotnet publish -c $BUILD_CONFIGURATION -o /app/publish -r linux-arm64

FROM scratch AS final
WORKDIR /app
COPY --from=build /app/publish .

