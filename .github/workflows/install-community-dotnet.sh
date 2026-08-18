#!/usr/bin/env bash

set -euo pipefail

export LC_ALL=C

if (( $# != 2 )); then
  printf 'Usage: %s <owner/repository> <asset-architecture>\n' "$0" >&2
  exit 2
fi

repository=$1
architecture=$2

if [[ ! $repository =~ ^[A-Za-z0-9_.-]+/[A-Za-z0-9_.-]+$ ]] ||
   [[ ! $architecture =~ ^[A-Za-z0-9_-]+$ ]]; then
  printf 'Invalid repository or asset architecture\n' >&2
  exit 2
fi

apt-get update
apt-get install -y --no-install-recommends binutils ca-certificates curl gcc coreutils jq libc-bin libc6-dev libgcc-s1 libgssapi-krb5-2 libicu76 libssl3t64 libstdc++6 libunwind8 linux-libc-dev tzdata zlib1g

curl_options=(--disable --proto '=https' --proto-redir '=https' --fail --location --retry 5 --silent --show-error)

candidates=$(mktemp)
trap 'rm -f "$candidates"' EXIT

page=1
while true; do
  releases=$(curl "${curl_options[@]}" \
    --header 'Accept: application/vnd.github+json' \
    --header 'X-GitHub-Api-Version: 2022-11-28' \
    "https://api.github.com/repos/$repository/releases?per_page=100&page=$page")

  release_count=$(jq -er 'if type == "array" then length else error("Expected a JSON array") end' <<< "$releases")
  jq -c --arg architecture "$architecture" '
    .[]
    | select(.draft | not)
    | .assets[]
    | select(.state == "uploaded")
    | . as $asset
    | select($asset.name | test("^dotnet-sdk-10\\.0\\.[0-9]+-linux-" + $architecture + "\\.tar\\.gz$"))
    | ($asset.name | capture("^dotnet-sdk-(?<version>10\\.0\\.[0-9]+)-linux-").version) as $version
    | { version: $version, parts: ($version | split(".") | map(tonumber)), url: $asset.browser_download_url }
  ' <<< "$releases" >> "$candidates"

  if (( release_count < 100 )); then
    break
  fi
  ((page += 1))
done

sdk=$(jq -ser '
  sort_by(.parts)
  | if length == 0 then error("No stable .NET 10 SDK asset found") else . end
  | (.[-1].parts) as $latest
  | [.[] | select(.parts == $latest)] | unique_by(.url)
  | if length == 1 then .[0] else error("Multiple URLs found for latest SDK") end
' "$candidates")
sdk_version=$(jq -er '.version | select(test("[[:cntrl:]]") | not)' <<< "$sdk")
sdk_url=$(jq -er '.url | select(test("[[:cntrl:]]") | not)' <<< "$sdk")

sdk_name=dotnet-sdk-$sdk_version-linux-$architecture.tar.gz
expected_url_prefix="https://github.com/$repository/releases/download/"
if [[ ! $sdk_version =~ ^10\.0\.[0-9]+$ ]] ||
   [[ $sdk_url != "$expected_url_prefix"*"/$sdk_name" ]] ||
   [[ $sdk_url == *[[:space:]]* ]]; then
  printf 'Invalid SDK release metadata for %s %s\n' "$repository" "$sdk_version" >&2
  exit 1
fi

printf 'Installing .NET SDK %s for %s from %s\n' "$sdk_version" "$architecture" "$repository"

sdk_archive=/tmp/$sdk_name
curl "${curl_options[@]}" --output "$sdk_archive" "$sdk_url"
mkdir -p /usr/share/dotnet
tar -xzf "$sdk_archive" -C /usr/share/dotnet
ln -s /usr/share/dotnet/dotnet /usr/bin/dotnet
