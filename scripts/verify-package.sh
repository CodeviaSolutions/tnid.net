#!/usr/bin/env bash
# Verify an existing archive without rebuilding or repacking the library.
set -euo pipefail
root=$(cd "$(dirname "$0")/.." && pwd)
tfm=${1:-net10.0}
package=${2:-$root/artifacts/TNID.NET.0.1.0.nupkg}
package=$(python3 -c 'import pathlib, sys; print(pathlib.Path(sys.argv[1]).resolve(strict=True))' "$package")
export DOTNET_CLI_HOME="${DOTNET_CLI_HOME:-${TMPDIR:-/tmp}/tnid-dotnet}"
export DOTNET_CLI_TELEMETRY_OPTOUT=1
export DOTNET_GENERATE_ASPNET_CERTIFICATE=false
export DOTNET_CLI_WORKLOAD_UPDATE_NOTIFY_DISABLE=true
case "$tfm" in
  net10.0) project="$root/tests/PackageConsumer"; sdk=dotnet ;;
  net11.0) project="$root/tests/PackageConsumer11"; sdk=${DOTNET11:-dotnet} ;;
  *) echo 'Usage: bash scripts/verify-package.sh [net10.0|net11.0] [package.nupkg]' >&2; exit 1 ;;
esac
work=$(mktemp -d "${TMPDIR:-/tmp}/tnid-packed-consumer.XXXXXX")
trap 'rm -rf "$work"' EXIT
mkdir -p "$work/feed"
cp "$package" "$work/feed/TNID.NET.0.1.0.nupkg"
cd "$project"
version=$("$sdk" --version)
echo "SDK: $version"
if [[ "$tfm" == net11.0 && "$version" != 11.0.100 ]]; then
  echo "Expected official SDK 11.0.100, got $version." >&2
  exit 1
fi
"$sdk" restore --source "$work/feed" --packages "$work/packages" --force --disable-build-servers
python3 "$root/scripts/check-consumer-assets.py" "$project/obj/project.assets.json" "$package" "$tfm"
"$sdk" run -c Release --no-restore --disable-build-servers
