#!/usr/bin/env bash
set -euo pipefail

export DEBIAN_FRONTEND=noninteractive

echo "Provisioning VM: update and install prerequisites"
apt-get update -y
apt-get install -y curl wget git ca-certificates build-essential libssl-dev unzip

# Install Docker (docker.io) and add vagrant to docker group
apt-get install -y docker.io
systemctl enable --now docker || true
usermod -aG docker vagrant || true

# Install Docker Compose CLI plugin binary for the VM architecture
COMPOSE_VERSION="v2.20.2"
ARCH=$(dpkg --print-architecture)
if [ "${ARCH}" = "arm64" ] || [ "${ARCH}" = "aarch64" ]; then
  COMPOSE_ARCH="aarch64"
elif [ "${ARCH}" = "amd64" ] || [ "${ARCH}" = "x86_64" ]; then
  COMPOSE_ARCH="x86_64"
else
  COMPOSE_ARCH="x86_64"
fi
mkdir -p /usr/local/lib/docker/cli-plugins
COMPOSE_PATH=/usr/local/lib/docker/cli-plugins/docker-compose
if [ ! -f "$COMPOSE_PATH" ]; then
  echo "Downloading docker compose ${COMPOSE_VERSION} for arch ${COMPOSE_ARCH}"
  curl -sSL "https://github.com/docker/compose/releases/download/${COMPOSE_VERSION}/docker-compose-linux-${COMPOSE_ARCH}" -o "$COMPOSE_PATH"
  chmod +x "$COMPOSE_PATH"
fi

DOTNET_INSTALL_DIR=/usr/share/dotnet
mkdir -p "$DOTNET_INSTALL_DIR"
curl -sSL https://dot.net/v1/dotnet-install.sh -o /tmp/dotnet-install.sh
chmod +x /tmp/dotnet-install.sh

/tmp/dotnet-install.sh --channel 9.0 --install-dir "$DOTNET_INSTALL_DIR" --architecture arm64 || \
  /tmp/dotnet-install.sh --channel 7.0 --install-dir "$DOTNET_INSTALL_DIR" --architecture arm64

ln -sf "$DOTNET_INSTALL_DIR/dotnet" /usr/bin/dotnet || true

echo "Building and starting DormCare"
if [ -f /vagrant/Docker-compose.yml ] || [ -f /vagrant/docker-compose.yml ]; then
  echo "Docker compose file found — starting containers"
  cd /vagrant
  # prefer exact filename case, fall back to lowercase
  if [ -f Docker-compose.yml ]; then
    docker compose -f Docker-compose.yml up -d
  else
    docker compose -f docker-compose.yml up -d
  fi
  echo "Docker containers started"
else
  if [ -d /vagrant/DormCare ]; then
    cd /vagrant/DormCare

    if [ -f DormCareSolution.sln ]; then
      $DOTNET_INSTALL_DIR/dotnet restore DormCareSolution.sln
      $DOTNET_INSTALL_DIR/dotnet build DormCareSolution.sln -c Release
    elif [ -f DormCare.csproj ]; then
      $DOTNET_INSTALL_DIR/dotnet restore DormCare.csproj
      $DOTNET_INSTALL_DIR/dotnet build DormCare.csproj -c Release
    else
      echo "No solution or project file found in /vagrant/DormCare"
    fi


    nohup $DOTNET_INSTALL_DIR/dotnet run --project DormCare.csproj --urls "http://0.0.0.0:2200" &> /var/log/dormcare.log &
    echo "DormCare started (logs: /var/log/dormcare.log)"
  else
    echo "Warning: /vagrant/DormCare not found. Ensure you run Vagrant from the project root."
  fi
fi
