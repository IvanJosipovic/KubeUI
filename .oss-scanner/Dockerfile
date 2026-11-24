FROM debian:trixie

ENV DEBIAN_FRONTEND=noninteractive \
    DOTNET_ROOT=/opt/dotnet \
    PATH="/opt/dotnet:${PATH}" \
    DOTNET_CLI_TELEMETRY_OPTOUT=1 \
    DOTNET_NOLOGO=1 \
    DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1 \
    NUGET_XMLDOC_MODE=skip

# Tooling and native libraries potentially required when the scanner
# exercises the Avalonia application.
RUN apt-get update \
    && apt-get install -y --no-install-recommends \
        ca-certificates \
        curl \
        git \
        python3 \
        libicu72 \
        libfontconfig1 \
        libfreetype6 \
        libice6 \
        libsm6 \
        libx11-6 \
        libxext6 \
        libxrender1 \
    && rm -rf /var/lib/apt/lists/*

# OSS Scanner contract: the repository checkout lives at /src.
COPY . /src
WORKDIR /src

# Install the exact .NET SDK specified by KubeUI's global.json.
RUN SDK_VERSION="$(python3 -c 'import json; print(json.load(open("global.json"))["sdk"]["version"])')" \
    && echo "Installing .NET SDK ${SDK_VERSION}" \
    && curl -fsSL https://dot.net/v1/dotnet-install.sh -o /tmp/dotnet-install.sh \
    && chmod +x /tmp/dotnet-install.sh \
    && /tmp/dotnet-install.sh \
        --version "${SDK_VERSION}" \
        --install-dir "${DOTNET_ROOT}" \
    && rm /tmp/dotnet-install.sh \
    && dotnet --info

# Fetch all NuGet packages before the scanner loses network access.
RUN dotnet restore KubeUI.slnx

# Build the complete project and retain symbols for analysis.
RUN dotnet build KubeUI.slnx \
        --configuration Release \
        --no-restore \
        --tl:off \
        -clp:ErrorsOnly \
        -p:DebugSymbols=true \
        -p:DebugType=portable

WORKDIR /src
