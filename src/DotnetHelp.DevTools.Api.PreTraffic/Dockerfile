FROM public.ecr.aws/lambda/provided:al2023 AS build
RUN dnf install clang-15.0.6-3.amzn2023.0.2.x86_64 --assumeyes
RUN dnf install libicu-67.1-7.amzn2023.0.3.x86_64 --assumeyes
RUN dnf install zlib-devel-1.2.11-33.amzn2023.0.4.x86_64 --assumeyes
RUN dnf install wget --assumeyes

RUN rpm --import https://packages.microsoft.com/keys/microsoft.asc
RUN wget -O /etc/yum.repos.d/microsoft-prod.repo https://packages.microsoft.com/config/fedora/37/prod.repo
RUN dnf install -y dotnet-sdk-8.0

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
WORKDIR /app
COPY --from=build /pre-traffic/publish .

