FROM public.ecr.aws/sam/build-provided.al2023:latest-x86_64 AS build

RUN dnf install clang libicu zlib-devel wget --assumeyes
RUN rpm --import https://packages.microsoft.com/keys/microsoft.asc
RUN wget -O /etc/yum.repos.d/microsoft-prod.repo https://packages.microsoft.com/config/fedora/37/prod.repo
RUN dnf install dotnet-sdk-8.0 --assumeyes

ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["DotnetHelp.DevTools.Api.PreTraffic/DotnetHelp.DevTools.Api.PreTraffic.csproj", "DotnetHelp.DevTools.Api.PreTraffic/"]
RUN dotnet restore "DotnetHelp.DevTools.Api.PreTraffic/DotnetHelp.DevTools.Api.PreTraffic.csproj"
COPY . .
WORKDIR "/src/DotnetHelp.DevTools.Api.PreTraffic"
RUN dotnet build "DotnetHelp.DevTools.Api.PreTraffic.csproj" -c $BUILD_CONFIGURATION -o /app/build

WORKDIR /src/DotnetHelp.DevTools.Api.PreTraffic
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish -c $BUILD_CONFIGURATION -o /pre-traffic/publish

FROM scratch AS final
WORKDIR /pre-traffic
COPY --from=build /pre-traffic/publish .

