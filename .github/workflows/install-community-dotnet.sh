#!/usr/bin/env bash

set -euo pipefail

export LC_ALL=C

archive=$1

apt-get update
apt-get install -y --no-install-recommends binutils ca-certificates gcc coreutils libc-bin libc6-dev libgcc-s1 libgssapi-krb5-2 libicu76 libssl3t64 libstdc++6 libunwind8 linux-libc-dev tzdata zlib1g

mkdir -p /usr/share/dotnet
tar -xzf "$archive" -C /usr/share/dotnet
ln -s /usr/share/dotnet/dotnet /usr/bin/dotnet
